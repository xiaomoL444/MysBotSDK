using MysBotSDK.MessageHandle;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
using vila_bot;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MysBotSDK.Connection.WebSocket;
using System.Security.Cryptography.X509Certificates;

namespace MysBotSDK
{
	public class MysBot : IDisposable
	{
		/// <summary>
		/// 是否选择ws连接
		/// </summary>
		public bool WebsocketConnect { private get; init; }
		public string? http_callback_Address { private get; init; }
		public string? ws_callback_Address { private get; init; }
		public string? bot_id { internal get; init; }
		public string? secret { internal get; init; }
		public string? pub_key { internal get; init; }
		public Logger.LoggerLevel loggerLevel { get { return Logger.loggerLevel; } set { Logger.loggerLevel = value; } }

		public IObservable<MessageReceiverBase> MessageReceiver => messageReceiver.AsObservable();
		private Subject<MessageReceiverBase> messageReceiver = new();

		private string? Certificate_Header { get; set; }

		private CancellationTokenSource tokenSource = new();

		private HttpListener? listener { get; set; }

		private WsClient wsClient { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public MysBot Initail()
		{
			Logger.Log("=========================================================\n初始化Bot");
			//检查bot参数是否齐全
			if (!(http_callback_Address != null || ws_callback_Address != null) || bot_id == null || secret == null | pub_key == null)
			{
				throw new Exception(Logger.LogError("Bot参数不齐全"));
			}
			MessageSender.MysBot = this;

			//设置鉴权请求头
			Certificate_Header = $@"x-rpc-bot_id:{bot_id}
x-rpc-bot_secret:{secret}
x-rpc-bot_villa_id:{Authentication.HmacSHA256(secret!, pub_key!)}";

			//创建监听
			if (!string.IsNullOrEmpty(http_callback_Address) && string.IsNullOrEmpty(ws_callback_Address))
			{
				Logger.Log("创建Http监听");
				listener = new HttpListener();
				listener.Prefixes.Add(http_callback_Address);
				listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
				listener.Start();

				_ = Task.Run(() =>
				{
					while (!tokenSource.IsCancellationRequested)
					{
						try
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

							Logger.Debug(data);

							//处理消息
							//校验伺服器请求头
							if (!Authentication.Verify(data, request.Headers.Get("x-rpc-bot_sign")!, pub_key!, secret!))
							{
								HttpRespond(response, new ResponseData() { message = "Invalid signature", retcode = 401 });
								Logger.LogWarnning("鉴权失败");
								continue;
							}

							MessageHandle(data);

							HttpRespond(response, new ResponseData() { message = "", retcode = 0 });
						}
						catch (Exception)
						{
							listener.Close();
						}
					}
					listener.Close();
				}, tokenSource.Token);
			}
			else if (!string.IsNullOrEmpty(ws_callback_Address) && string.IsNullOrEmpty(http_callback_Address))
			{
				Logger.Log("创建websocker监听");
				Func<Task?> func = null!;
				func = (async () =>
				{
					bool isNeedReconnect = false;
					WebSocketSharp.WebSocket webSocket = new WebSocketSharp.WebSocket(ws_callback_Address);

					webSocket.OnOpen += (sender, e) => { Logger.Log("开启websocket连接"); };
					webSocket.OnMessage += (sender, e) =>
					{
						if (e.IsText)
						{
							string data = e.Data;
							Logger.Debug(data);

							//没有鉴权
							MessageHandle(data);
						}
					};
					webSocket.OnError += (sender, e) => { Logger.LogError($"websocket出现错误{e.Message}\n{e.Exception}"); };
					webSocket.OnClose += (sender, e) =>
					{
						Logger.Log("websocket关闭，尝试重新连接");
						isNeedReconnect = true;
						Task.Run(func);
					};
					webSocket.Connect();
					while (!isNeedReconnect && !tokenSource.IsCancellationRequested)
					{
						//保活，30s发送一次消息
						webSocket.Send("BALUS");
						await Task.Delay(1000 * 30, tokenSource.Token);
					}
				});
				_ = Task.Run(func, tokenSource.Token);
			}
			else
			{
				throw new Exception("不能同时传入ws_callback与http_callback,或者没有传值");
			}
			if (WebsocketConnect)
			{
				//开启官方的ws连接
				_ = Task.Run(() =>
				{
					wsClient = new WsClient(bot_id, secret!, 8489);
				});
			}
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
		public void MessageHandle(string data)
		{
			//解析消息
			try
			{
				MessageReceiverBase messageReceiverBase = new MessageReceiverBase(data);

				//接收器
				MessageReceiverBase? MsgRecvBase;
				//写一段解析json消息查看事件是什么，再将Json消息丢入构造器中
				switch ((EventType)((int)JObject.Parse(data)["event"]!["type"]!))
				{
					case EventType.JoinVilla:
						messageReceiver.OnNext(new JoinVillaReceiver(data));
						break;
					case EventType.SendMessage:
						messageReceiver.OnNext(new SendMessageReceiver(data));
						break;
					case EventType.CreateRobot:
						messageReceiver.OnNext(new CreateRobotReceiver(data));
						break;
					case EventType.DeleteRobot:
						messageReceiver.OnNext(new DeleteRobotReceiver(data));
						break;
					case EventType.AddQuickEmoticon:
						messageReceiver.OnNext(new AddQuickEmoticonReceiver(data));
						break;
					case EventType.AuditCallback:
						messageReceiver.OnNext(new AuditCallbackReceiver(data));
						break;
					case EventType.ClickMsgComponent:
						messageReceiver.OnNext(new ClickMsgComponentReceiver(data));
						break;
					default:
						break;
				}
			}
			catch (Exception e)
			{

				Logger.LogError("解析消息失败" + e.StackTrace);
			}
		}

		/// <summary>
		/// 针对ws连接下的消息处理
		/// </summary>
		/// <param name="robotEventMessage"></param>
		public void MessageHandle(RobotEventMessage robotEventMessage)
		{


			//解析消息
			try
			{
				MessageReceiverBase messageReceiverBase = new MessageReceiverBase(robotEventMessage);
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
					case EventType.ClickMsgComponent:
						messageReceiver.OnNext((ClickMsgComponentReceiver)messageReceiverBase.receiver);
						break;
					default:
						break;
				}
			}
			catch (Exception e)
			{

				Logger.LogError("解析消息失败" + e.StackTrace);
			}
		}

		public void Dispose()
		{
			tokenSource.Cancel();
			if (listener != null) listener.Close();
			tokenSource.Dispose();
			MessageSender.mysBot.Remove(this);
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