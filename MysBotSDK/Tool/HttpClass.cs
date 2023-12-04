using System.Net.Http.Headers;
using System.Net;

namespace MysBotSDK.Tool;

public static class HttpClass
{
	private static HttpClientHandler clientHandle;
	private static HttpClient client;

	/// <summary>
	/// 静态构造器
	/// </summary>
	static HttpClass()
	{
		clientHandle = new HttpClientHandler()
		{
			MaxConnectionsPerServer = 1,
			UseCookies = true,
			AutomaticDecompression = DecompressionMethods.GZip,
			UseProxy = false,
			Proxy = null,
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
	}

	/// <summary>
	/// 异步发送Http消息
	/// </summary>
	/// <param name="httpRequestMessage">请求消息</param>
	/// <returns>Http回复消息</returns>
	public static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage)
	{
		try
		{
			var res = await client.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);
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
