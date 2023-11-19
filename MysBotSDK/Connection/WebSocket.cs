using Google.Protobuf;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using VilaBot;

namespace MysBotSDK.Connection
{
	internal class WebSocket
	{
		public async Task Connect(string bot_id, string bot_secret, uint villa_id = 0)
		{
			var res = await GetWebSocketInfo(bot_id, bot_secret, villa_id);
			if (res.retcode != 0)
			{
				Logger.LogWarnning($"连接失败{res}");
				return;
			}
			_ = Task.Run(() =>
			{
				WebSocketSharp.WebSocket webSocket = new WebSocketSharp.WebSocket(res.websocket_url);
				webSocket.OnOpen += (sender, e) =>
				{
					//伺服器开启
				};
				webSocket.OnMessage += (sedner, e) =>
				{
					if (e.IsBinary)
					{
						//解析数据
					}
				};
				webSocket.OnClose += (sedner, e) =>
				{
					//伺服器关闭
				};
				webSocket.OnError += (sender, e) =>
				{
					//发生错误
				};
			});
			return;
		}

		public async Task<(string message, int retcode, string websocket_url, UInt64 websocket_conn_uid, Int32 app_id, Int32 platform, string device_id)> GetWebSocketInfo(string bot_id, string bot_secret, uint villa_id = 0)
		{
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetWebSocketInfo);

			httpRequestMessage.AddHeaders(@$"x-rpc-bot_id:{bot_id}
x-rpc-bot_secret:{bot_secret}
x-rpc-bot_villa_id:{villa_id}
x-rpc-bot_ts:{(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMicroseconds}
x-rpc-bot_nonce:{Guid.NewGuid()}
Content-Type:application/json");

			var res = await HttpClass.SendAsync(httpRequestMessage);

			var AnonymousType = new
			{
				retcode = 0,
				message = "",
				data = new
				{
					websocket_url = "",
					websocket_conn_uid = (UInt64)0,
					app_id = 0,
					platform = 0,
					device_id = ""
				}
			};
			var json = JsonConvert.DeserializeAnonymousType(await res.Content.ReadAsStringAsync(), AnonymousType)!;

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


		public async Task Login(UInt64 uid, UInt64 villa_id, string secret, string bot_id, int app_id, string device_id, int platform)
		{
			PLogin pLogin = new PLogin()
			{
				Uid = 0,
				Token = $"{villa_id}.{secret}.{bot_id}",
				AppId = app_id,
				DeviceId = device_id,
				Platform = platform,
				Region = string.Empty   //?
			};
			var serialization = pLogin.ToByteArray();

			return;
		}
	}
}
