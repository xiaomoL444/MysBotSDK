﻿using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using MysBotSDK.Connection;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using System;
using VilaBot;

namespace MysBotSDK.Connection.WebSocket;

internal static class ExtensionEndianMethod
{
	/// <summary>
	/// 转为小端字序
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
	/// <summary>
	/// 转为大端口(其实不需要的)字序
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public static byte[] ToBigEndiam(this byte[] source)
	{
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(source);
		}
		return source;
	}
}

internal class WsClient
{
	/// <summary>
	/// 是否连接失败
	/// </summary>
	public bool isConnectFail = false;

	/// <summary>
	/// websocker连接实例
	/// </summary>
	WebSocketSharp.WebSocket? webSocket;

	/// <summary>
	/// 心跳包计时器
	/// </summary>
	System.Timers.Timer? heartBeatTimer;

	/// <summary>
	/// 若心跳包收到错误消息后进行兜底，未发出的消息放在这里
	/// </summary>
	List<byte[]> RetryMsg = new List<byte[]>();

	public WsClient(string bot_id, string bot_secret, uint villa_id = 0)
	{
		var wsInfo = GetWebSocketInfo(bot_id, bot_secret, villa_id);
		//若返回值不等于0，错误
		if (wsInfo.retcode != 0)
		{
			Logger.LogWarnning($"连接失败{wsInfo}");
			isConnectFail = true;
			return;
		}
		_ = Task.Run(() =>
		{
			webSocket = new WebSocketSharp.WebSocket(wsInfo.websocket_url);

			webSocket.OnOpen += (sender, e) =>
			{
				//伺服器开启
				Login(wsInfo.websocket_conn_uid, villa_id, bot_secret, bot_id, wsInfo.app_id, wsInfo.device_id, wsInfo.platform);
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
			webSocket.OnClose += (sedner, e) =>
			{
				//伺服器关闭
				Logger.LogWarnning("伺服器关闭!");
			};
			webSocket.OnError += (sender, e) =>
			{
				//发生错误
				Logger.LogError($"发生未知错误{e.Message}\n{e.Exception}");
			};
		});
		return;
	}

	public (string message, int retcode, string websocket_url, ulong websocket_conn_uid, int app_id, int platform, string device_id) GetWebSocketInfo(string bot_id, string bot_secret, uint villa_id = 0)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetWebSocketInfo);

		httpRequestMessage.AddHeaders(@$"x-rpc-bot_id:{bot_id}
x-rpc-bot_secret:{bot_secret}
x-rpc-bot_villa_id:{villa_id}
x-rpc-bot_ts:{(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMicroseconds}
x-rpc-bot_nonce:{Guid.NewGuid()}
Content-Type:application/json");

		var res = HttpClass.SendAsync(httpRequestMessage).Result;

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new
			{
				websocket_url = "",
				websocket_conn_uid = (ulong)0,
				app_id = 0,
				platform = 0,
				device_id = ""
			}
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType)!;

		try
		{
			//返回获取的数值
			return new()
			{
				retcode = json.retcode,
				message = json.message,
				websocket_url = json.data.websocket_url,
				websocket_conn_uid = json.data.websocket_conn_uid,
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
		webSocket!.Send(bytes.ToArray());

		return;
	}
	#endregion

	#region 接收消息
	internal (ulong id, uint flag) LastMsg;
	public void ReceiveMsg(byte[] data)
	{
		//解析
		WebSocketMessage wsMsg = new WebSocketMessage()
		{
			Magic = BitConverter.ToUInt32(data.Take(4).ToArray()),//用于标识报文的开始 目前的协议的magic值是十六进制的【0xBABEFACE】
			DataLen = BitConverter.ToUInt32(data.Skip(4).Take(4).ToArray()),//变长部分总长度=变长头长度+变长消息体长度
			HeaderLen = BitConverter.ToUInt32(data.Skip(8).Take(4).ToArray()),//变长头总长度，变长头部分所有字段（包括HeaderLen本身）的总长度。
			ID = BitConverter.ToUInt32(data.Skip(12).Take(4).ToArray()),//协议包序列ID，同一条连接上的发出的协议包应该单调递增，相同序列ID且Flag字段相同的包应该被认为是同一个包
			Flag = BitConverter.ToUInt32(data.Skip(20).Take(4).ToArray()),//配合bizType使用，用于标识同一个bizType协议的方向。用 1 代表主动发到服务端的request包用 2 代表针对某个request包回应的response包
			BizType = BitConverter.ToUInt32(data.Skip(24).Take(4).ToArray()),//消息体的业务类型，用于标识Body字段中的消息所属业务类型
			AppId = BitConverter.ToUInt32(data.Skip(28).Take(4).ToArray()),//应用标识。固定为 104

		};

		//相同序列ID且Flag字段相同的包应该被认为是同一个包
		if (LastMsg == (wsMsg.ID, wsMsg.Flag))
		{
			Logger.Debug("收到相同的包，跳过");
			return;
		}

		//HandleMessage
		switch (wsMsg.BizType)//唔姆唔姆...因为command里面没有RobotEvent所以用数字不用枚举了...
		{
			case 30001://RobotEvent
				break;
			case 7://PLoginReply
				LoginReply(wsMsg);
				break;
			case 8://PLogoutReply
				LogoutReply(wsMsg);
				break;
			case 6://PHeartBeatReply
				break;
			case 53://PKickOff
				break;
			case 52://Shutdown
				break;
			default:
				Logger.LogError($"异常数据BizType = {wsMsg.BizType} ，请检查是数据异常还是暂未支持的数据类型");
				break;
		}
		return;
	}
	#endregion

	#region 发送命令
	private void Login(ulong uid, ulong villa_id, string secret, string bot_id, int app_id, string device_id, int platform)
	{
		PLogin pLogin = new PLogin()
		{
			Uid = uid,
			Token = $"{villa_id}.{secret}.{bot_id}",
			AppId = app_id,
			DeviceId = device_id,
			Platform = platform,
			Region = string.Empty   //?
		};

		SendMsg(Command.PLogin, pLogin, (uint)app_id, MsgType.Request);
		return;
	}
	private void Logout(UInt64 uid, Int32 platform, Int32 app_id, string device_id)
	{
		PLogout pLogout = new PLogout()
		{
			Uid = uid,
			Platform = platform,
			AppId = app_id,
			DeviceId = device_id
		};
		SendMsg(Command.Logout, pLogout, (uint)app_id, MsgType.Request);
		return;
	}
	private void HeartBeat(Int32 app_id)
	{
		PHeartBeat pHeartBeat = new PHeartBeat
		{
			ClientTimestamp = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMicroseconds.ToString()
		};
		SendMsg(Command.Heartbeat, pHeartBeat, (uint)app_id, MsgType.Request);
	}
	#endregion

	#region 接收命令
	public void LoginReply(WebSocketMessage wsMsg)
	{
		PLoginReply pLoginReply = PLoginReply.Parser.ParseFrom(wsMsg.BodyData);
		switch (pLoginReply.Code)
		{
			case 0:
				Logger.Log("WebSocket连接成功");
				//登录成功，进行操作,启动计时器每二十秒发送一次心跳包
				heartBeatTimer = new System.Timers.Timer(20 * 1000);
				heartBeatTimer.Elapsed += (sender, e) =>
				{
					HeartBeat((int)wsMsg.AppId);
				};
				heartBeatTimer.Start();
				isConnectFail = false;

				break;
			case 1000://登录失败，参数错误
				Logger.LogError("登录失败，参数错误");
				isConnectFail = true;
				break;
			case 1001://登录失败，系统错误
				Logger.LogError("登录失败，系统错误");
				isConnectFail = true;
				break;
			default:
				Logger.LogWarnning("无法识别的返回码");
				break;
		}
	}

	public void LogoutReply(WebSocketMessage wsMsg)
	{
		PLogoutReply pLogoutReply = PLogoutReply.Parser.ParseFrom(wsMsg.BodyData);
		switch (pLogoutReply.Code)
		{
			case 0:
				Logger.Log("WebSocket登出成功");
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
	#endregion

}