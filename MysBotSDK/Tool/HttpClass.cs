using System.Net.Http.Headers;
using System.Net;
using System.Reflection;
using MysBotSDK.MessageHandle;

namespace MysBotSDK.Tool;

/// <summary>
/// 一个发送消息的类
/// </summary>
public static class HttpClass
{
	private static HttpClientHandler clientHandle;
	private static HttpClient client;

	/// <summary>
	/// 每秒最大查询数量
	/// </summary>
	public const int QPS = 20;

	/// <summary>
	/// 该秒已发送的数量
	/// </summary>
	static int had_sended_queue_nums_per_second = 0;

	/// <summary>
	/// 目前可以执行的TaskID
	/// </summary>
	static UInt128 execute_task_ID = 0;

	/// <summary>
	/// 目前存在最新的id
	/// </summary>
	static UInt128 last_task_ID = 0;

	static Task detectTask;//用于探测发送数量并及时止损(
	static System.Timers.Timer clearQueueTimer;//用于每秒清零已发送的数量

	/// <summary>
	/// 静态构造器
	/// </summary>
	static HttpClass()
	{
		clientHandle = new HttpClientHandler()
		{
			MaxConnectionsPerServer = QPS,
			UseCookies = true,
			AutomaticDecompression = DecompressionMethods.GZip,
			UseProxy = false,
			Proxy = null,
			//UseProxy = true,
			//Proxy = new WebProxy(new Uri("http://localhost:8888")),
			UseDefaultCredentials = true,
			AllowAutoRedirect = true,
			ClientCertificateOptions = ClientCertificateOption.Automatic,
			ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; },
		};
		client = new HttpClient(clientHandle) { Timeout = new TimeSpan(0, 1, 0) };

		CacheControlHeaderValue cacheControl = new CacheControlHeaderValue();
		cacheControl.NoCache = true;
		cacheControl.NoStore = true;
		client.DefaultRequestHeaders.CacheControl = cacheControl;

		client.DefaultRequestHeaders.Connection.Add("keep-alive");

		//设置一个任务，用于定时清零已发送的数量
		clearQueueTimer = new System.Timers.Timer(1000);
		clearQueueTimer.Elapsed += (sender, e) =>
		{
			//若上一秒没有队列消息
			//if (had_sended_queue_nums_per_second == 0)
			//{
			//	//清零任务ID
			//	last_task_ID = 0;
			//	execute_task_ID = 0;
			//}

			//Logger.Log($"{execute_task_ID}");
			had_sended_queue_nums_per_second = 0;
		};
		clearQueueTimer.Start();

		//设置发送的队列ID
		detectTask = Task.Run(async () =>
		{
			while (true)
			{
				//	Logger.Log($"{had_sended_queue_nums_per_second}");
				if (had_sended_queue_nums_per_second < QPS && execute_task_ID < last_task_ID)
				{
					had_sended_queue_nums_per_second++;//队列数量增加
													   //await Task.Delay(1000 / QPS);
					execute_task_ID++;
				}
				await Task.Delay(1);
			}
		});
	}

	private const string SEND_STATUS_FIELD_NAME = "_sendStatus";
	/// <summary>
	/// 异步发送Http消息
	/// </summary>
	/// <param name="httpRequestMessage">请求消息</param>
	/// <returns>Http回复消息</returns>
	public static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage)
	{
		try
		{
			//若有新的任务
			UInt128 self_task_ID = last_task_ID++;//获取自身的任务id

			//判断自己的ID是否可以执行
			while (execute_task_ID < self_task_ID) await Task.Delay(1);//Logger.LogWarnning($"{execute_task_ID} {self_task_ID}"); ;

			var res = await client.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);

			if (res.StatusCode == HttpStatusCode.TooManyRequests)
			{
				Logger.LogWarnning("请求过快，重新请求");

				TypeInfo requestType = httpRequestMessage.GetType().GetTypeInfo();
				FieldInfo sendStatusField = requestType.GetField(SEND_STATUS_FIELD_NAME, BindingFlags.Instance | BindingFlags.NonPublic);
				if (sendStatusField != null)
					sendStatusField.SetValue(httpRequestMessage, 0);
				else
					Logger.LogError($"Failed to hack HttpRequestMessage, {SEND_STATUS_FIELD_NAME} doesn't exist.");

				res = await SendAsync(httpRequestMessage);
			}
			return res;
		}
		catch (Exception e)
		{
			Logger.LogError($"消息发送失败\n{e.StackTrace}");
			return null;
		}


	}

	/// <summary>
	/// 为请求信息添加请求头
	/// </summary>
	/// <param name="httpRequestMessage">请求信息</param>
	/// <param name="cookies">请求头</param>
	/// <returns>已添加请求头的Http请求信息</returns>
	public static HttpRequestMessage AddHeaders(this HttpRequestMessage httpRequestMessage, string cookies)
	{
		StringReader stringReader = new StringReader(cookies);
		string? temp_str;

		while ((temp_str = stringReader.ReadLine()) != null)//分割请求头，以第一个:为分割符
		{
			try
			{
				if (temp_str.EndsWith(";"))
				{
					temp_str = temp_str.Substring(0, temp_str.Length - 1);
				}
				int index = temp_str.IndexOf(':');
				string name = temp_str.Substring(0, index);
				string value = temp_str.Substring(index + 1, temp_str.Length - 1 - index);
				//httpRequestMessage.Headers.Add(name,value);
				httpRequestMessage.Headers.TryAddWithoutValidation(name, value);
			}
			catch (Exception)
			{
				Logger.LogError($"分割请求头失败temp_str:{temp_str}，跳过该请求头");
			}

		}
		return httpRequestMessage;
	}
}