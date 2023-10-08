using MysBotSDK.MessageHandle;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK
{
	public class MysBot
	{
		public string callback_Adress { private get; init; }
		public string bot_id { internal get; init; }
		public string secret { internal get; init; }
		public string pub_key { internal get; init; }
		public Logger.LoggerLevel loggerLevel { get { return Logger.loggerLevel; } set { Logger.loggerLevel = value; } }

		public CancellationTokenSource CancellationTokenSource { get; private init; }

		public Action<MessageReceiver> JoinVilla { get; set; }
		public Action<MessageReceiver> SendMessage { get; set; }
		public Action<MessageReceiver> CreateRobot { get; set; }
		public Action<MessageReceiver> DeleteRobot { get; set; }
		public Action<MessageReceiver> AddQuickEmoticon { get; set; }
		public Action<MessageReceiver> AuditCallback { get; set; }
		private string Certificate_Header { get; set; }

		public MysBot()
		{
			CancellationTokenSource = new CancellationTokenSource();
		}

		public MysBot Initail()
		{
			//检查bot参数是否齐全
			if (callback_Adress == null || bot_id == null || secret == null | pub_key == null)
			{
				throw new Exception(Logger.LogError("Bot参数不齐全"));
			}
			MessageSender.MysBot = this;

			//设置鉴权请求头
			Certificate_Header = $@"x-rpc-bot_id:{bot_id}
x-rpc-bot_secret:{secret}
x-rpc-bot_villa_id:{Authentication.HmacSHA256(secret, pub_key)}";

			//创建监听
			HttpListener listener = new HttpListener();
			listener.Prefixes.Add(callback_Adress);
			listener.Start();
			_ = Task.Run(() =>
			{
				while (true)
				{
					var content = listener.GetContext();
					var request = content.Request;
					var response = content.Response;

					if (request.HttpMethod != "POST")
					{
						HttpRespond(response, new ResponseData() { message = "Method Was Not Allow", retcode = 400 });
						continue;
					}
					//获取信息流
					var steam = request.InputStream;
					var reader = new StreamReader(steam);
					var data = reader.ReadToEnd();

					//处理消息
					//校验伺服器请求头
					if (!Authentication.Verify(data, request.Headers.Get("x-rpc-bot_sign"), pub_key, secret))
					{
						HttpRespond(response, new ResponseData() { message = "Invalid signature", retcode = 401 });
						Logger.LogWarnning("鉴权失败");
						continue;
					}

					//解析消息
					try
					{
						MessageReceiver messageReceiver = new MessageReceiver(data);
						switch (messageReceiver.EventType)
						{
							case EventType.JoinVilla:
								JoinVilla.Invoke(messageReceiver);
								break;
							case EventType.SendMessage:
								SendMessage.Invoke(messageReceiver);
								break;
							case EventType.CreateRobot:
								CreateRobot.Invoke(messageReceiver);
								break;
							case EventType.DeleteRobot:
								DeleteRobot.Invoke(messageReceiver);
								break;
							case EventType.AddQuickEmoticon:
								AddQuickEmoticon.Invoke(messageReceiver);
								break;
							case EventType.AuditCallback:
								AuditCallback.Invoke(messageReceiver);
								break;
							default:
								break;
						}
					}
					catch (Exception e)
					{

						Logger.LogError("解析消息失败" + e.StackTrace);
					}
					HttpRespond(response, new ResponseData() { message = "", retcode = 0 });
				}
			}, CancellationTokenSource.Token);
			return this;
		}

		/// <summary>
		/// 回应我吧，月下初拥！
		/// </summary>
		/// <param name="response"></param>
		/// <param name="responseString">回应的消息，丢入一个Json消息即可</param>
		private void HttpRespond(HttpListenerResponse listenerResponse, ResponseData responseString)
		{
			var output = listenerResponse.OutputStream;
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString.ToString());
			listenerResponse.ContentLength64 = buffer.Length;
			output.Write(buffer, 0, buffer.Length);
		}

	}
}
public class ResponseData
{
	public string message { get; set; } = "";
	public int retcode { get; set; } = 0;

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this).ToString();
	}
}