using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using MysBotSDK.Connection;
using MysBotSDK.MessageHandle;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Text;
using VilaBot;

namespace MysBotSDK.Connection.WebSocket;

internal static class ExtensionEndianMethod
{
	/// <summary>
	/// 发送时转为小端字序
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public static byte[] ToLittleEndian(this byte[] source)
	{
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse(source);
		}
		return source;
	}
}

internal class WsClient : IDisposable
{
	/// <summary>
	/// Bot实例
	/// </summary>
	private MysBot? mysBot { get; init; }

	/// <summary>
	/// 是否连接失败
	/// </summary>
	public bool isConnectFail { get; set; } = false;

	/// <summary>
	/// websocker连接实例
	/// </summary>
	WebSocketSharp.WebSocket? webSocket { get; set; }

	/// <summary>
	/// 心跳包计时器
	/// </summary>
	System.Timers.Timer? heartBeatTimer;

	/// <summary>
	/// 若心跳包收到错误消息后进行兜底，未发出的消息放在这里
	/// </summary>
	List<byte[]> RetryMsg = new List<byte[]>();

	/// <summary>
	/// 心跳包发送是否失败，已用来判断是否兜底消息
	/// </summary>
	bool isHeartBeatFail { get; set; } = false;

	/// <summary>
	/// 上一段心跳包伺服器的时间
	/// </summary>
	UInt64 Last_server_timestamp { get; set; }

	/// <summary>
	/// 是否强制关闭连接并不再重连
	/// </summary>
	bool isForceDisConnect { get; set; } = false;

	/// <summary>
	/// 链接信息
	/// </summary>
	private (UInt64 uid, Int32 platform, Int32 app_id, string device_id) linkInfo { get; set; }

	public UInt64 GetCurrentTime()
	{
		return (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
	}

	public WsClient(MysBot mysBot, string bot_id, string bot_secret, uint villa_id = 0)
	{
		this.mysBot = mysBot;
		//连接委托
		Func<Task?> func = null!;
		func = (async () =>
		{
			//获取ws链接
			Logger.Debug("异步获取websocketInfo中...");
			(string message, int retcode, string websocket_url, ulong websocket_conn_uid, int app_id, int platform, string device_id) wsInfo;
			try
			{
				wsInfo = await GetWebSocketInfo(bot_id, bot_secret, villa_id);
			}
			catch (Exception)
			{
				Logger.LogWarnning("获取websocketInfo失败！五秒后重新连接");
				await Task.Delay(5 * 1000);
				await Task.Run(func);
				return;
			}
			Logger.Debug($"返回值{wsInfo}");
			//若返回值不等于0，错误
			if (wsInfo.retcode != 0)
			{
				Logger.LogWarnning($"连接失败{wsInfo}，五秒后尝试重新连接");
				isConnectFail = true;
				await Task.Delay(5 * 1000);
				await Task.Run(func);
				return;
			}

			linkInfo = (wsInfo.websocket_conn_uid, wsInfo.platform, wsInfo.app_id, wsInfo.device_id);

			if (webSocket != null)
			{
				if (webSocket.IsAlive)
				{
					Logger.LogWarnning("已存在一个存活的连接，关闭上一个连接");
					webSocket.Close();
				}
			}
			webSocket = new WebSocketSharp.WebSocket(wsInfo.websocket_url);
			webSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

			webSocket.OnOpen += (sender, e) =>
			{
				//伺服器开启
				Login(villa_id, bot_secret, bot_id);
				//输入登录指令，失败没有登录(懒)
			};
			webSocket.OnMessage += (sedner, e) =>
			{
				if (e.IsBinary)
				{
					//解析数据
					ReceiveMsg(e.RawData);
				}
			};
			webSocket.OnClose += async (sedner, e) =>
			{
				//伺服器关闭
				Logger.LogWarnning("连接关闭，若不是伺服器请求的关闭，则五秒后尝试重连");
				if (isForceDisConnect)
				{
					Logger.Log("伺服器强制关闭");
					return;
				}
				await Task.Delay(5 * 1000);
				await Task.Run(func);
			};
			webSocket.OnError += (sender, e) =>
			{
				//发生错误
				Logger.LogError($"发生未知错误{e.Message}\n{e.Exception}\n出现错误并没有重连机制");
			};

			try
			{
				//开启连接
				webSocket.Connect();
			}
			catch (TimeoutException)
			{
				Logger.LogError("连接超时，五秒后重新连接");
				await Task.Delay(5 * 1000);
				await Task.Run(func);
			}
		});
		Logger.Log("委托设置完成");
		_ = Task.Run(func);
		Logger.Log("委托运行");
		return;
	}

	public async Task<(string message, int retcode, string websocket_url, ulong websocket_conn_uid, int app_id, int platform, string device_id)> GetWebSocketInfo(string bot_id, string bot_secret, uint villa_id = 0)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetWebSocketInfo);

		httpRequestMessage.AddHeaders(@$"x-rpc-bot_id:{bot_id}
x-rpc-bot_secret:{bot_secret}
x-rpc-bot_villa_id:{villa_id}
x-rpc-bot_ts:{(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMicroseconds}
x-rpc-bot_nonce:{Guid.NewGuid()}
Content-Type:application/json");

		var res = await HttpClass.SendAsync(httpRequestMessage);

		Logger.Debug(await res.Content.ReadAsStringAsync());

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new
			{
				websocket_url = "",
				uid = (ulong)0,
				app_id = 0,
				platform = 0,
				device_id = ""
			}
		};
		var json = JsonConvert.DeserializeAnonymousType(await (res.Content.ReadAsStringAsync()), AnonymousType)!;

		try
		{
			//返回获取的数值
			return new()
			{
				retcode = json.retcode,
				message = json.message,
				websocket_url = json.data.websocket_url,
				websocket_conn_uid = json.data.uid,
				app_id = json.data.app_id,
				platform = json.data.platform,
				device_id = json.data.device_id
			};
		}
		catch (Exception)
		{
			//返回错误值
			return new()
			{
				retcode = json.retcode,
				message = json.message
			};
		}
	}

	#region 发送消息
	public enum MsgType
	{
		Request = 1,
		Response = 2
	}
	private ulong uniqMsgType = 1;
	public ulong UniqMsgType { get { return ++uniqMsgType; } }
	public void SendMsg<T>(Command bizType, T protobufData, uint appid = 104, MsgType msgType = MsgType.Request) where T : IMessage
	{
		try
		{
			var bodyData = protobufData.ToByteArray();
			WebSocketMessage wsMsg = new()
			{
				BodyData = bodyData,

				Magic = 0xBABEFACE,
				DataLen = (uint)(24 + bodyData.Length),
				HeaderLen = 24,
				ID = UniqMsgType,
				Flag = (uint)msgType,
				BizType = (uint)bizType,
				AppId = appid
			};
			List<byte> bytes = new List<byte>();

			//添加字段
			bytes.AddRange(BitConverter.GetBytes(wsMsg.Magic).ToLittleEndian());
			bytes.AddRange(BitConverter.GetBytes(wsMsg.DataLen).ToLittleEndian());
			bytes.AddRange(BitConverter.GetBytes(wsMsg.HeaderLen).ToLittleEndian());
			bytes.AddRange(BitConverter.GetBytes(wsMsg.ID).ToLittleEndian());
			bytes.AddRange(BitConverter.GetBytes(wsMsg.Flag).ToLittleEndian());
			bytes.AddRange(BitConverter.GetBytes(wsMsg.BizType).ToLittleEndian());
			bytes.AddRange(BitConverter.GetBytes(wsMsg.AppId).ToLittleEndian());
			bytes.AddRange(wsMsg.BodyData.ToLittleEndian());

			//发送
			if (isConnectFail && !bizType.Equals(Command.Heartbeat))
			{
				//心跳包发送失败，消息兜底
				Logger.LogWarnning($"ws消息兜底 Flag [ {msgType} ]ID [ {UniqMsgType} ] ");
				RetryMsg.Add(bytes.ToArray());
				return;
			}
			if (!isConnectFail && RetryMsg.Count != 0)
			{
				//心跳包恢复，将兜底的信息重新发送
				RetryMsg.ForEach(retryMsg => { webSocket!.Send(retryMsg.ToArray()); });
				RetryMsg.Clear();
			}
			if (!webSocket!.IsAlive)
			{
				Logger.LogWarnning("ws已关闭或不存活");
				return;
			}
			Logger.Log($"ws发送Flag [ {msgType} ] ID [ {UniqMsgType} ]");
			webSocket!.Send(bytes.ToArray());
		}
		catch (Exception e)
		{
			Logger.LogError($"ws发送消息失败...? {e.Message}");
		}
		return;
	}
	#endregion

	#region 接收消息
	internal (ulong id, uint flag) LastMsg;
	public void ReceiveMsg(byte[] data)
	{
		try
		{
			//解析
			WebSocketMessage wsMsg = new WebSocketMessage()
			{
				Magic = BitConverter.ToUInt32(data.Take(4).ToArray()),//用于标识报文的开始 目前的协议的magic值是十六进制的【0xBABEFACE】
				DataLen = BitConverter.ToUInt32(data.Skip(4).Take(4).ToArray()),//变长部分总长度=变长头长度+变长消息体长度
				HeaderLen = BitConverter.ToUInt32(data.Skip(8).Take(4).ToArray()),//变长头总长度，变长头部分所有字段（包括HeaderLen本身）的总长度。
				ID = BitConverter.ToUInt64(data.Skip(12).Take(8).ToArray()),//协议包序列ID，同一条连接上的发出的协议包应该单调递增，相同序列ID且Flag字段相同的包应该被认为是同一个包
				Flag = BitConverter.ToUInt32(data.Skip(20).Take(4).ToArray()),//配合bizType使用，用于标识同一个bizType协议的方向。用 1 代表主动发到服务端的request包用 2 代表针对某个request包回应的response包
				BizType = BitConverter.ToUInt32(data.Skip(24).Take(4).ToArray()),//消息体的业务类型，用于标识Body字段中的消息所属业务类型
				AppId = BitConverter.ToUInt32(data.Skip(28).Take(4).ToArray()),//应用标识。固定为 104
			};
			wsMsg.BodyData = data.Skip(32).Take((int)(wsMsg.DataLen - wsMsg.HeaderLen)).ToArray();

			//若本机为大端，则转换字序
			if (!BitConverter.IsLittleEndian)
			{
				wsMsg.Magic = BitConverter.ToUInt32(BitConverter.GetBytes(wsMsg.Magic).Reverse().ToArray());
				wsMsg.DataLen = BitConverter.ToUInt32(BitConverter.GetBytes(wsMsg.DataLen).Reverse().ToArray());
				wsMsg.HeaderLen = BitConverter.ToUInt32(BitConverter.GetBytes(wsMsg.HeaderLen).Reverse().ToArray());
				wsMsg.ID = BitConverter.ToUInt64(BitConverter.GetBytes(wsMsg.ID).Reverse().ToArray());
				wsMsg.Flag = BitConverter.ToUInt32(BitConverter.GetBytes(wsMsg.Flag).Reverse().ToArray());
				wsMsg.BizType = BitConverter.ToUInt32(BitConverter.GetBytes(wsMsg.BizType).Reverse().ToArray());
				wsMsg.AppId = BitConverter.ToUInt32(BitConverter.GetBytes(wsMsg.AppId).Reverse().ToArray());
				wsMsg.BodyData = wsMsg.BodyData.Reverse().ToArray();
			}

			//相同序列ID且Flag字段相同的包应该被认为是同一个包
			if (LastMsg == (wsMsg.ID, wsMsg.Flag))
			{
				Logger.Debug("收到相同的包，跳过");
				return;
			}


			Logger.Log($"收到伺服器发送来的BizType为 [ {wsMsg.BizType} ] ID为 [ {wsMsg.ID} ] 的协议包");
			//HandleMessage
			switch (wsMsg.BizType)//唔姆唔姆...因为command里面没有RobotEvent所以用数字不用枚举了...
			{
				case 30001://RobotEvent
					RobotEventHandle(wsMsg);
					break;
				case 7://PLoginReply
					LoginReply(wsMsg);
					break;
				case 8://PLogoutReply
					LogoutReply(wsMsg);
					break;
				case 6://PHeartBeatReply
					HeartBeatReply(wsMsg);
					break;
				case 53://PKickOff
					KickOff(wsMsg);
					break;
				case 52://Shutdown
					Shutdown(wsMsg);
					break;
				default:
					Logger.LogError($"解析异常数据BizType = {wsMsg.BizType} ，请检查是数据异常还是暂未支持的数据类型,尝试解析通用返回");
					CommonReplyHandle(wsMsg);
					break;
			}
		}
		catch (Exception e)
		{
			Logger.LogError($"ws解析失败...?{e.Message},{e.StackTrace}");
		}
		return;
	}
	#endregion

	#region 发送命令
	private void Login(ulong villa_id, string secret, string bot_id)
	{
		PLogin pLogin = new PLogin()
		{
			Uid = linkInfo.uid,
			Token = $"{villa_id}.{secret}.{bot_id}",
			AppId = linkInfo.app_id,
			DeviceId = linkInfo.device_id,
			Platform = linkInfo.platform,
			Region = string.Empty   //?
		};

		SendMsg(Command.PLogin, pLogin, (uint)linkInfo.app_id, MsgType.Request);
		return;
	}
	private void Logout()
	{
		PLogout pLogout = new PLogout()
		{
			Uid = linkInfo.uid,
			Platform = linkInfo.platform,
			AppId = linkInfo.app_id,
			DeviceId = linkInfo.device_id
		};
		SendMsg(Command.Logout, pLogout, (uint)linkInfo.app_id, MsgType.Request);
		return;
	}
	private void HeartBeat(Int32 app_id)
	{
		PHeartBeat pHeartBeat = new PHeartBeat
		{
			ClientTimestamp = GetCurrentTime().ToString()
		};
		SendMsg(Command.PHeartbeat, pHeartBeat, (uint)app_id, MsgType.Request);
	}
	#endregion

	#region 接收命令
	private void CommonReplyHandle(WebSocketMessage wsMsg)
	{
		CommonReply commonReply = CommonReply.Parser.ParseFrom(wsMsg.BodyData);

		Logger.Log($"通用解析结果{commonReply}");
	}
	/// <summary>
	/// 收到事件回调
	/// </summary>
	/// <param name="wsMsg"></param>
	private void RobotEventHandle(WebSocketMessage wsMsg)
	{
		RobotEvent robotEventMessage = RobotEvent.Parser.ParseFrom(wsMsg.BodyData);

		Logger.Debug(robotEventMessage.ToString());

		//组装一条json消息
		var packMsg = new
		{
			@event = new
			{
				robot = robotEventMessage.Robot,
				type = robotEventMessage.Type,
				extend_data = new
				{
					EventData = robotEventMessage.ExtendData
				},
				create_at = robotEventMessage.CreatedAt,
				id = robotEventMessage.Id,
				send_at = robotEventMessage.SendAt
			}
		};

		mysBot?.MessageHandle(JsonConvert.SerializeObject(packMsg, new JsonSerializerSettings
		{
			ContractResolver = new DefaultContractResolver()
			{
				NamingStrategy = new LowCamelCaseToUnderscore()
			}
		}));

	}
	/// <summary>
	/// 收到登录回应
	/// </summary>
	/// <param name="wsMsg">wss消息体</param>
	public void LoginReply(WebSocketMessage wsMsg)
	{
		PLoginReply pLoginReply = PLoginReply.Parser.ParseFrom(wsMsg.BodyData);
		switch (pLoginReply.Code)
		{
			case 0:
				Logger.Log("WebSocket连接成功");
				//储存当前时间
				Last_server_timestamp = GetCurrentTime();
				//重置布尔值状态
				isForceDisConnect = false;
				isHeartBeatFail = false;
				//登录成功，进行操作,启动计时器每十秒发送一次心跳包
				heartBeatTimer?.Stop();
				heartBeatTimer = new System.Timers.Timer(10 * 1000);
				heartBeatTimer.Elapsed += (sender, e) =>
				{
					if ((GetCurrentTime() - Last_server_timestamp) / (Math.Pow(10, 3)) >= 60)
					{
						//心跳包超过60秒没有恢复，断开连接(onClose里调用重新连接)，同时计时器停止
						webSocket?.Close();
						heartBeatTimer.Stop();
					}
					HeartBeat((int)wsMsg.AppId);
				};
				heartBeatTimer.Start();
				isConnectFail = false;

				break;
			case 1000://登出失败，参数错误
				Logger.LogError("登出失败，参数错误");
				isConnectFail = true;
				break;
			case 1001://登出失败，系统错误
				Logger.LogError("登出失败，系统错误");
				isConnectFail = true;
				break;
			default:
				Logger.LogWarnning("无法识别的返回码");
				break;
		}
	}

	/// <summary>
	/// 收到登出回应(断开连接不再重连)
	/// </summary>
	/// <param name="wsMsg">wss消息体</param>
	public void LogoutReply(WebSocketMessage wsMsg)
	{
		PLogoutReply pLogoutReply = PLogoutReply.Parser.ParseFrom(wsMsg.BodyData);
		switch (pLogoutReply.Code)
		{
			case 0:
				Logger.Log("WebSocket登出成功");
				isForceDisConnect = true;
				webSocket?.Close();
				if (heartBeatTimer != null)
				{
					//关闭计时器
					heartBeatTimer.Stop();
				}
				break;
			case 1000://登录失败，参数错误
				Logger.LogError("登录失败，参数错误");
				break;
			case 1001://登录失败，系统错误
				Logger.LogError("登录失败，系统错误");
				break;
			default:
				Logger.LogWarnning("无法识别的返回码");
				break;
		}
	}

	/// <summary>
	/// 收到心跳包回应
	/// </summary>
	/// <param name="wsMsg"></param>
	public void HeartBeatReply(WebSocketMessage wsMsg)
	{
		PHeartBeatReply pHeartBeatReply = PHeartBeatReply.Parser.ParseFrom(wsMsg.BodyData);
		switch (pHeartBeatReply.Code)
		{
			case 0:
				//Logger.Debug("收到心跳包");
				//Logger.Debug($"{pHeartBeatReply.ServerTimestamp}");
				Last_server_timestamp = pHeartBeatReply.ServerTimestamp;
				isHeartBeatFail = false;
				break;
			case 1000://心跳包错误，参数错误
				Logger.LogError("心跳包错误，参数错误");
				isHeartBeatFail = true;
				break;
			case 1001://心跳包错误，系统错误
				Logger.LogError("心跳包错误，系统错误");
				isHeartBeatFail = true;
				break;
			default:
				Logger.LogWarnning("无法识别的返回码");
				isHeartBeatFail = true;
				break;
		}
	}

	/// <summary>
	/// 收到踢出下线回应(断开连接不再重连)
	/// </summary>
	/// <param name="wsMsg"></param>
	public void KickOff(WebSocketMessage wsMsg)
	{
		PKickOff pKickOff = PKickOff.Parser.ParseFrom(wsMsg.BodyData);
		switch (pKickOff.Code)
		{
			case 0:
				Logger.LogWarnning($"收到踢出登录的消息，状态码 {pKickOff.Code} ，原因 {pKickOff.Reason}");
				//关闭wss连接
				isForceDisConnect = true;
				webSocket?.Close();
				if (heartBeatTimer != null)
				{
					//关闭计时器
					heartBeatTimer.Stop();
				}
				break;
			case 1000://登录失败，参数错误
				Logger.LogError("踢出失败，参数错误");
				break;
			case 1001://登录失败，系统错误
				Logger.LogError("踢出失败，系统错误");
				break;
			default:
				Logger.LogWarnning("无法识别的返回码");
				break;
		}
	}

	/// <summary>
	/// 伺服器发来关机信息(需要断开链接并进行重连)
	/// </summary>
	/// <param name="wsMsg"></param>
	public void Shutdown(WebSocketMessage wsMsg)
	{
		Logger.Log("伺服器发来关闭消息，即将重新连接");
		//计时器停止
		if (heartBeatTimer != null)
		{
			heartBeatTimer.Stop();
		}
		//重置布尔值状态
		isForceDisConnect = false;
		isHeartBeatFail = false;
		//清空兜底信息
		RetryMsg.Clear();
		//关闭连接，将自动重新连接
		webSocket?.Close();
	}

	public void Dispose()
	{
		Logger.Log("释放WebSocket资源");
		isForceDisConnect = true;
		//伺服器关闭
		if (webSocket != null && webSocket.IsAlive)
		{
			webSocket.Close();
			webSocket = null;
		}
		//定时器关闭
		if (heartBeatTimer != null)
		{
			heartBeatTimer.Stop();
			heartBeatTimer.Dispose();
			heartBeatTimer = null;
		}
		//兜底的消息清空
		RetryMsg.Clear();
	}
	#endregion

}
class LowCamelCaseToUnderscore : NamingStrategy
{
	protected override string ResolvePropertyName(string name)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < name.Length; i++)
		{
			if (65 <= name[i] && name[i] <= 90)
			{
				if (i != 0)
				{
					stringBuilder.Append($"_");
				}
				stringBuilder.Append((char)(name[i] + 32));
			}
			else
			{
				stringBuilder.Append($"{name[i]}");
			}
		}
		return stringBuilder.ToString();
	}
}