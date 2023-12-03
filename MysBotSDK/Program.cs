using MysBotSDK.Tool;
using Newtonsoft;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Net.Sockets;

namespace MysBotSDK;

internal class Program
{
	static MysBot? mysBot;
	public static async Task Main(string[] args)
	{
		string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
		Logger.Log("========================================================");
		Logger.Log($"当前MysSDK版本:{ver}");

		Logger.Log($"加载Bot信息");

		LoadBotConfig();

		while (true)
		{

		}
	}
	internal static void LoadBotConfig()
	{
		if (!File.Exists("account.json"))
		{
			Logger.LogWarnning("不存在Bot配置信息，即将创建一个account.json");

			FileStream fileStream = new FileStream("account.json", FileMode.CreateNew, FileAccess.ReadWrite);
			StreamWriter streamWriter = new StreamWriter(fileStream);
			streamWriter.Write(JsonConvert.SerializeObject(new
			{
				WebsocketConnect = false,
				test_villa_id = 0,
				loggerLevel = 3,
				ws_callback_Address = "",
				http_callback_Address = "",
				bot_id = "",
				secret = "",
				pub_key = ""
			}));
			streamWriter.Close();
			fileStream.Close();

			Logger.LogWarnning("创建成功，按任意键退出程序，退出后填写配置单再次启动");
			Console.ReadKey();
			Environment.Exit(0);
		}
		try
		{
			mysBot = new MysBot()//末酱
			{
				WebsocketConnect = bool.Parse(GetAccountConfig("WebsocketConnect")),
				test_villa_id = uint.Parse(GetAccountConfig("test_villa_id")),
				loggerLevel = (Logger.LoggerLevel)uint.Parse(GetAccountConfig("loggerLevel")),
				ws_callback_Address = GetAccountConfig("ws_callback_Address"),
				http_callback_Address = GetAccountConfig("http_callback_Address"),
				bot_id = GetAccountConfig("bot_id"),
				secret = GetAccountConfig("secret"),
				pub_key = GetAccountConfig("pub_key").Replace(@"\n", "\n").TrimEnd(' ')
			}.Initail();
		}
		catch (Exception ex)
		{
			Logger.LogError($"错误信息 {ex.Message} \n加载Bot失败，按任意键退出程序");
			Console.ReadKey();
			Environment.Exit(0);

		}
	}
	/*

	public static void LoadPlugins(out WeakReference weakReference)
	{

	}
	*/

	public static string GetAccountConfig(string key)
	{
		string account_path = "./account.json";
		var dic = FileHandle.ReadAsDicString(account_path);
		if (!dic.ContainsKey(key))
		{
			dic[key] = string.Empty;
			FileHandle.SaveDicString(account_path, dic);
		}

		return dic[key];
	}

	public static UInt64 GetCurrentTime()
	{
		return (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMicroseconds;
	}
}
