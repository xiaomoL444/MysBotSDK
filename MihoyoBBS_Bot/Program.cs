using MysBotSDK;
using MysBotSDK.MessageHandle;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace MihoyoBBS_Bot;
static class Program
{
	public static async Task Main()
	{
		//初始化Bot
		MysBot mysBot = new MysBot()
		{
			callback_Adress = "http://127.0.0.1:12328/",
			bot_id = "bot_D0Vo7OZqT0LZR3MQUkTZ",
			secret = "mIC6cdoSjOXCxkFijqa905S1bqdMVoIdHAQzFf5GnKb9e",
			pub_key = @"-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDmnRn8xpyiSxf1w101sBSybU23
Wd5lbo3aBquzaUqJHqpG9FaAc2vxMtXmOmnde0x2rXUmEOUqeOBm3ES+dBU2OhhK
XngKV1pzuABsnPpcBGf47a9dJYALyzs8IVwl1SsKQgAAV8g/jtf4H2DjQLG8R9cc
uktHkKy3hPOs5V9HuwIDAQAB
-----END PUBLIC KEY-----
"
		}.Initail();

		//读取程序集
		List<IMysPluginModule> plugins = new List<IMysPluginModule>();

		AssemblyLoadContext assemblyLoadContext = new AssemblyLoadContext("Plugins", true);

		if (!Directory.Exists("Plugins"))
		{
			Logger.Log("不存在路径./Plugins/，即将创建文件夹");
			Directory.CreateDirectory("Plugins");
		}
		var assemblies_path = Directory.EnumerateFiles("./Plugins", "*.dll");
		foreach (var assembly_path in assemblies_path)
		{
			try
			{
				var steam = new FileStream(assembly_path, FileMode.Open);
				assemblyLoadContext.LoadFromStream(steam);
				steam.Close();
			}
			catch (Exception)
			{
				Logger.LogError($"加载{assembly_path}失败");
			}
		}
		List<IMysPluginModule> mysPluginModules = new List<IMysPluginModule>();
		List<IMysPluginModule> JoinVilla = new List<IMysPluginModule>();
		List<IMysPluginModule> SendMessage = new List<IMysPluginModule>();
		List<IMysPluginModule> CreateRobot = new List<IMysPluginModule>();
		List<IMysPluginModule> DeleteRobot = new List<IMysPluginModule>();
		List<IMysPluginModule> AddQuickEmoticon = new List<IMysPluginModule>();
		List<IMysPluginModule> AuditCallback = new List<IMysPluginModule>();

		//加载插件
		foreach (var assembly in assemblyLoadContext.Assemblies)
		{
			var rawModules = assembly.GetTypes().Where(x => x.GetInterfaces().Any(i => i == typeof(IMysPluginModule))).ToList();
			if (rawModules.Count == 0) continue;
			mysPluginModules.AddRange(rawModules.Select(Activator.CreateInstance).Select(m => (m as IMysPluginModule)!));
		}
		foreach (var mysPluginModule in mysPluginModules)
		{
			Logger.Log($"搜索到方法[{String.Join("", mysPluginModule.GetType().CustomAttributes)}] {mysPluginModule.GetType().Name}");
		}
		JoinVilla.AddRange(mysPluginModules.Where(q => q.GetType().GetCustomAttributes<JoinVillaAttribute>() != null));
		SendMessage.AddRange(mysPluginModules.Where(q => q.GetType().GetCustomAttributes<SendMessageAttribute>() != null));
		CreateRobot.AddRange(mysPluginModules.Where(q => q.GetType().GetCustomAttributes<CreateRobotAttribute>() != null));
		DeleteRobot.AddRange(mysPluginModules.Where(q => q.GetType().GetCustomAttributes<DeleteRobotAttribute>() != null));
		AddQuickEmoticon.AddRange(mysPluginModules.Where(q => q.GetType().GetCustomAttributes<AddQuickEmoticonAttribute>() != null));
		AuditCallback.AddRange(mysPluginModules.Where(q => q.GetType().GetCustomAttributes<AuditCallbackAttribute>() != null));

		mysBot.JoinVilla += (data) => { Array.ForEach(JoinVilla.ToArray(), method => { if (method.Enable) { method.Execute(data); } }); };
		mysBot.SendMessage += (data) =>
		{
			Array.ForEach(SendMessage.ToArray(), method =>
			{
				if (method.Enable)
				{
					var commond = data.SendMessage.content.content.text.Split(" ").ToList();
					commond.RemoveAt(0);
					if (commond[0] == $"/{method.GetType().GetCustomAttribute<SendMessageAttribute>().Commond}" || commond[0] == $"{method.GetType().GetCustomAttribute<SendMessageAttribute>().Commond}")
					{
						Logger.Log($"Commond:{method.GetType().GetCustomAttribute<SendMessageAttribute>().Commond}");
						method.Execute(data);
					}

				}
			});
		};
		mysBot.CreateRobot += (data) => { Array.ForEach(CreateRobot.ToArray(), method => { if (method.Enable) { method.Execute(data); } }); };
		mysBot.DeleteRobot += (data) => { Array.ForEach(DeleteRobot.ToArray(), method => { if (method.Enable) { method.Execute(data); } }); };
		mysBot.AddQuickEmoticon += (data) => { Array.ForEach(AddQuickEmoticon.ToArray(), method => { if (method.Enable) { method.Execute(data); } }); };
		mysBot.AuditCallback += (data) => { Array.ForEach(AuditCallback.ToArray(), method => { if (method.Enable) { method.Execute(data); } }); };

		await Commond();
	}
	/// <summary>
	/// 终端命令
	/// </summary>
	static async Task Commond()
	{
		await Task.Run(() =>
			{
				while (true)
				{
					string? commond = Console.ReadLine();
					string?[] commondSplit = commond.Split(" ");
					Logger.Log($"input {commond}");
					Type type = typeof(Commond);
					MethodInfo? methodInfo;
					try
					{
						methodInfo = type.GetMethod(commondSplit[0]);
					}
					catch (ArgumentException)
					{
						Logger.LogError($"没有此方法！");
						continue;
					}

					if (String.IsNullOrWhiteSpace(commond)) continue;

					try
					{
						methodInfo.Invoke(null, new object[] { commondSplit });
					}
					catch (ArgumentException e)
					{
						Console.WriteLine(e.Message);
						continue;
					}
					catch (NullReferenceException e)
					{
						Console.WriteLine(e.Message);
						continue;
					}
				}
			});
	}
}
static class Commond
{
	public static void help(string?[] args)
	{
		Type type = typeof(Commond);
		Console.WriteLine("========================");
		foreach (var method in type.GetMethods())
		{
			if (method.Name == "GetType" || method.Name == "ToString" || method.Name == "Equals" || method.Name == "GetHashCode") continue;
			Console.WriteLine(method.Name);
		}
		Console.WriteLine("========================");
	}
	//public static void updata(string?[] args)
	//{
	//	Program.UpdatePlugin();
	//}
	//public static void exit(string?[] args)
	//{
	//	Program.Modules.Where(q => q.GetType().GetCustomAttribute<StopAttribute>() != null).ToList().Raise(null);
	//	Environment.Exit(0);
	//}
}
