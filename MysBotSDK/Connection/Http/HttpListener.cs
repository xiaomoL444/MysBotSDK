using MysBotSDK.Tool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.Connection.Http;

internal class HttpListener : IDisposable
{
	/// <summary>
	/// http侦听器
	/// </summary>
	private System.Net.HttpListener? listener { get; set; }

	private CancellationTokenSource tokenSource = new();

	public HttpListener(MysBot mysBot, string http_callback_Address, string secret, string pub_key)
	{
		listener = new System.Net.HttpListener();
		listener.Prefixes.Add(http_callback_Address);
		listener.AuthenticationSchemes = System.Net.AuthenticationSchemes.Anonymous;
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

					mysBot.MessageHandle(data);

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
	/// <summary>
	/// 回应我吧，月下初拥！
	/// </summary>
	/// <param name="listenerResponse">一个收到的回应消息</param>
	/// <param name="responseString">回应的消息，丢入一个Json消息即可</param>
	private void HttpRespond(System.Net.HttpListenerResponse listenerResponse, ResponseData responseString)
	{
		var output = listenerResponse.OutputStream;
		byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString.ToString());
		listenerResponse.ContentLength64 = buffer.Length;
		output.Write(buffer, 0, buffer.Length);
	}

	public void Dispose()
	{
		tokenSource.Cancel();
		if (listener != null && listener.IsListening) listener.Close();
		tokenSource.Dispose();
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
}
