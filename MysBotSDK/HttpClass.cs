using System.Net.Http.Headers;
using System.Net;

namespace MysBotSDK;

public static class HttpClass
{
	private static HttpClientHandler clientHandle;
	private static HttpClient client;
	static HttpClass()
	{
		clientHandle = new HttpClientHandler()
		{
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
	public static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage)
	{
		var res = await client.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);
		return res;
	}
	public static HttpRequestMessage AddHeaders(this HttpRequestMessage httpRequestMessage, string cookies)
	{
		StringReader stringReader = new StringReader(cookies);
		string? temp_str;
		while ((temp_str = stringReader.ReadLine()) != null)
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
		return httpRequestMessage;
	}
}
