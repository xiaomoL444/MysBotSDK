using MysBotSDK.Connection.Http;
using MysBotSDK.Connection.WebSocket;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
using Newtonsoft.Json.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MysBotSDK
{
	public class MysBot : IDisposable
	{
		/// <summary>
		/// 是否选择ws连接
		/// </summary>
		public bool WebsocketConnect { private get; init; }
		/// <summary>
		/// 若选择ws连接，若bot未上架，则填入测试的别野id，上架的话则不用填入
		/// </summary>
		public uint test_villa_id { private get; init; }
		/// <summary>
		/// http回调地址
		/// </summary>
		public string? http_callback_Address { private get; init; }
		/// <summary>
		/// ws连接回调地址
		/// </summary>
		[Obsolete]
		public string? ws_callback_Address { private get; init; }
		/// <summary>
		/// Bot的id
		/// </summary>
		public string? bot_id { internal get; init; }
		/// <summary>
		/// bot的secret
		/// </summary>
		public string? secret { internal get; init; }
		/// <summary>
		/// bot的pub_key
		/// </summary>
		public string? pub_key { internal get; init; }
		/// <summary>
		/// 日志输出等级
		/// </summary>
		public Logger.LoggerLevel loggerLevel { get { return Logger.loggerLevel; } set { Logger.loggerLevel = value; } }

		/// <summary>
		/// 消息接收器
		/// </summary>
		public IObservable<MessageReceiverBase> MessageReceiver => messageReceiver.AsObservable();
		private Subject<MessageReceiverBase> messageReceiver = new();

		private string? Certificate_Header { get; set; }

		private HttpListener? httpListener { get; set; }
		/// <summary>
		/// ws连接实例
		/// </summary>
		private WsClient? wsClient { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public MysBot Initail()
		{
			Logger.Log("=========================================================\n初始化Bot");
			//检查bot参数是否齐全
			if (bot_id == null || secret == null || pub_key == null)
			{
				throw new Exception(Logger.LogError("Bot参数不齐全"));
			}

			//静态发送器添加该Bot
			MessageSender.MysBot = this;

			//设置鉴权请求头
			Certificate_Header = $@"x-rpc-bot_id:{bot_id}
x-rpc-bot_secret:{secret}
x-rpc-bot_villa_id:{Authentication.HmacSHA256(secret!, pub_key!)}";

			//创建监听
			//若http回调地址不为空，创建http侦听器
			if (!string.IsNullOrEmpty(http_callback_Address))
			{
				Logger.Log("创建Http监听");
				httpListener = new(this, http_callback_Address, secret, pub_key);
			}
			//若ws连接不为空，则创建ws连接实例
			else if (!string.IsNullOrEmpty(ws_callback_Address))
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
					while (!isNeedReconnect)
					{
						//保活，30s发送一次消息
						webSocket.Send("BALUS");
						await Task.Delay(1000 * 30);
					}
				});
				_ = Task.Run(func);
			}
			//若websocketConnect为true
			else if (WebsocketConnect)
			{
				Logger.Log("创建ws连接(官方)");
				//开启官方的ws连接
				wsClient = new WsClient(this, bot_id, secret!, test_villa_id);
			}
			//若什么都没有开启
			else
			{
				throw new Exception("请检查是否添加了回调地址，或者检查WebsocketConnect是否开启");
			}

			return this;
		}
		/// <summary>
		/// 消息解释器
		/// </summary>
		/// <param name="data"></param>
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
		/// 删除方法
		/// </summary>
		public void ClearHandle()
		{
			if (messageReceiver == null) return;
			messageReceiver.Dispose();
			messageReceiver = null!;
		}
		/// <summary>
		/// Dispose方法
		/// </summary>
		public void Dispose()
		{
			ClearHandle();
			if (httpListener != null)
			{
				httpListener.Dispose();
			}
			if (wsClient != null)
			{
				wsClient.Dispose();
			}

			MessageSender.mysBot.Remove(this);
		}
	}
}