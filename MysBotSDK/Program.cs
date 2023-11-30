using Google.Protobuf;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.ExtendData;
using MysBotSDK.MessageHandle.Info;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace MysBotSDK;

internal class Program
{
	public static async Task Main(string[] args)
	{
		string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
		Logger.Log("========================================================");
		Logger.Log($"当前MysSDK版本:{ver}");

		var mysBot = new MysBot()//末酱映射机
		{
			loggerLevel = Logger.LoggerLevel.Debug,
			http_callback_Address = "http://127.0.0.1:12328/",
			bot_id = "bot_w56csMVkMDa5efrcv7mg",
			secret = "JoACslgqTb8cYyNar11ont3N64Q78ilkqe4BHQrX7asRL",
			pub_key = "-----BEGIN PUBLIC KEY-----\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDX/+DXkGYlR8QBRnB5WUKhaWd1\npom1PKAXvR9GIpFt9bFJR8GRQwnl3rJc6YXQZOhOT+W3G4KG4TTQUUQBiyodqjRu\ngYP5XMj7LJDybl/v0Rl5pQCZlXpqNWL8lq6GjlV3Y3AZ5J7px0OLP7vJlp/CAQK3\nLMawok8dvtW6G0I9OQIDAQAB\n-----END PUBLIC KEY-----\n"
		}.Initail();

		int size_count = 6;
		int result_count = 0;
		string message = string.Empty;
		int retcode = 0;
		string offset_str = string.Empty;
		List<Member> members = new List<Member>();
		do
		{
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetVillaMember);
			httpRequestMessage.AddHeaders(MessageSender.FormatHeader(mysBot, 8489));
			httpRequestMessage.Content = JsonContent.Create(new { size = size_count, offset_str });

			var res = await HttpClass.SendAsync(httpRequestMessage);
			Logger.Debug($"获取大别野成员信息{res.Content.ReadAsStringAsync().Result}");
			var AnonymousType = new
			{
				retcode = 0,
				message = "",
				data = new { next_offset_str = "", list = new List<Member>() }
			};
			var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
			retcode = json!.retcode;
			message = json.message;

			//如果获取失败退出循环
			if (json.data == null)
			{
				break;
			}

			result_count = json.data.list.Count;
			members.AddRange(json.data.list);
			offset_str = json.data.next_offset_str;
		} while (result_count == size_count && retcode == 0);



		while (true)
		{ }
	}
	public static UInt64 GetCurrentTime()
	{
		return (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMicroseconds;
	}
}
