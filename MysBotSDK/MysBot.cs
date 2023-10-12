using MysBotSDK.MessageHandle;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;

namespace MysBotSDK
{
    public class MysBot
	{
		public string callback_Adress { private get; init; }
		public string bot_id { internal get; init; }
		public string secret { internal get; init; }
		public string pub_key { internal get; init; }
		public Logger.LoggerLevel loggerLevel { get { return Logger.loggerLevel; } set { Logger.loggerLevel = value; } }

		public IObservable<MessageReceiverBase> MessageReceiver => messageReceiver.AsObservable();
		private Subject<MessageReceiverBase> messageReceiver = new();

		private string Certificate_Header { get; set; }

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

					Logger.Debug(data);

					//解析消息
					try
					{
						MessageReceiverBase messageReceiverBase = new MessageReceiverBase(data);
						//MessageReceiver应该是一个抽象类(父类)，然后下面就替换成事件触发器

						switch (messageReceiverBase.EventType)
						{
							case EventType.JoinVilla:
								messageReceiver.OnNext((JoinVillaReceiver)messageReceiverBase.receiver);
								break;
							case EventType.SendMessage:
								messageReceiver.OnNext((SendMessageReceiver)messageReceiverBase.receiver);
								break;
							case EventType.CreateRobot:
								messageReceiver.OnNext((CreateRobotReceiver)messageReceiverBase.receiver);
								break;
							case EventType.DeleteRobot:
								messageReceiver.OnNext((DeleteRobotReceiver)messageReceiverBase.receiver);
								break;
							case EventType.AddQuickEmoticon:
								messageReceiver.OnNext((AddQuickEmoticonReceiver)messageReceiverBase.receiver);
								break;
							case EventType.AuditCallback:
								messageReceiver.OnNext((AuditCallbackReceiver)messageReceiverBase.receiver);
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
			});
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