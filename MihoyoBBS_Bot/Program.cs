using MysBotSDK;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace MihoyoBBS_Bot;
static class Program
{
	public static string GetAccountConfig(string key)
	{
		string account_path = "./account.json";
		var dic = FileHandle.ReadAsDicString(account_path);
		if (!dic.ContainsKey(key))
		{
			dic[key] = "";
			FileHandle.SaveDicString(account_path, dic);
		}

		return dic[key];
	}
	public static async Task Main()
	{
		//初始化Bot
		MysBot mysBot;
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{

			Action saveAction = new Action(() => { });

			mysBot = new MysBot()//末酱映射机
			{
				loggerLevel = Logger.LoggerLevel.Debug,
				http_callback_Address = GetAccountConfig("http_callback_Address"),
				bot_id = GetAccountConfig("mojiang_ready_bot_id"),
				secret = GetAccountConfig("mojiang_ready_secret"),
				pub_key = GetAccountConfig("mojiang_ready_pub_key").Replace("\\n", "\n").TrimEnd(' ')
			}.Initail();
		}
		else
		{
			mysBot = new MysBot()//末酱
			{
				loggerLevel = Logger.LoggerLevel.Log,
				ws_callback_Address = GetAccountConfig("ws_callback_Address"),
				bot_id = GetAccountConfig("mojiang_bot_id"),
				secret = GetAccountConfig("mojiang_secret"),
				pub_key = GetAccountConfig("mojiang_pub_key").Replace("\\n", "\n").TrimEnd(' ')
			}.Initail();
		}

		//读取程序集
		List<IMysPluginModule> plugins = new List<IMysPluginModule>();

		AssemblyLoadContext assemblyLoadContext = new AssemblyLoadContext("Plugins", true);

		if (!Directory.Exists("Plugins"))
		{
			Logger.Log("不存在路径./Plugins/，即将创建文件夹");
			Directory.CreateDirectory("Plugins");
		}
		var assemblies_path = Directory.EnumerateFiles("./Plugins", "*.dll").ToList();
		//assemblies_path.AddRange(Directory.EnumerateFiles("./", "*.dll").ToList());
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
				Logger.LogError($"加载插件{assembly_path}失败");
			}
		}

		List<IMysPluginModule> mysPluginModules = new List<IMysPluginModule>();
		List<IProgramPluginModule> programPluginMoudles = new List<IProgramPluginModule>();
		List<IProgramPluginModule> Start = new List<IProgramPluginModule>();

		List<IMysPluginModule> JoinVilla = new List<IMysPluginModule>();
		List<IMysPluginModule> SendMessage = new List<IMysPluginModule>();
		List<IMysPluginModule> CreateRobot = new List<IMysPluginModule>();
		List<IMysPluginModule> DeleteRobot = new List<IMysPluginModule>();
		List<IMysPluginModule> AddQuickEmoticon = new List<IMysPluginModule>();
		List<IMysPluginModule> AuditCallback = new List<IMysPluginModule>();

		//加载插件
		foreach (var assembly in assemblyLoadContext.Assemblies)
		{
			try
			{
				var rawModules = assembly.GetTypes().Where(x => x.GetInterfaces().Any(i => i == typeof(IMysPluginModule) || i == typeof(IProgramPluginModule))).ToList();
				if (rawModules.Count == 0) continue;
				mysPluginModules.AddRange(rawModules.Where(q => q.GetInterfaces().Any(i => i == typeof(IMysPluginModule))).Select(Activator.CreateInstance).Select(m => (m as IMysPluginModule)!));
				programPluginMoudles.AddRange(rawModules.Where(q => q.GetInterfaces().Any(i => i == typeof(IProgramPluginModule))).Select(Activator.CreateInstance).Select(m => (m as IProgramPluginModule)!));
			}
			catch (Exception)
			{
				Logger.LogError($"读取程序集{assembly.FullName}错误");
			}

		}
		foreach (var mysPluginModule in mysPluginModules)
		{
			Logger.Log($"搜索到方法[{String.Join("", mysPluginModule.GetType().CustomAttributes)}] {mysPluginModule.GetType().Name}");
		}
		foreach (var programPluginModule in programPluginMoudles)
		{
			Logger.Log($"搜索到方法[{String.Join("", programPluginModule.GetType().CustomAttributes)}] {programPluginModule.GetType().Name}");

		}


		if (programPluginMoudles.Count > 0)
		{
			Start = new List<IProgramPluginModule>(programPluginMoudles.Where(q => q.GetType().GetCustomAttributes<StartAttribute>() != null));
			programPluginMoudles.Clear();
		}
		if (mysPluginModules.Count > 0)
		{
			JoinVilla = new List<IMysPluginModule>(mysPluginModules.Where(q => q.GetType().GetCustomAttribute<JoinVillaAttribute>() != null));
			SendMessage = new List<IMysPluginModule>(mysPluginModules.Where(q => q.GetType().GetCustomAttribute<SendMessageAttribute>() != null));
			CreateRobot = new List<IMysPluginModule>(mysPluginModules.Where(q => q.GetType().GetCustomAttribute<CreateRobotAttribute>() != null));
			DeleteRobot = new List<IMysPluginModule>(mysPluginModules.Where(q => q.GetType().GetCustomAttribute<DeleteRobotAttribute>() != null));
			AddQuickEmoticon = new List<IMysPluginModule>(mysPluginModules.Where(q => q.GetType().GetCustomAttribute<AddQuickEmoticonAttribute>() != null));
			AuditCallback = new List<IMysPluginModule>(mysPluginModules.Where(q => q.GetType().GetCustomAttribute<AuditCallbackAttribute>() != null));
			mysPluginModules.Clear();
		}
		Array.ForEach(Start.ToArray(), method => { if (method.Enable) { method.Execute(); } });
		Array.ForEach(JoinVilla.ToArray(), method => { mysBot.MessageReceiver.OfType<JoinVillaReceiver>().Subscribe(async (receiver) => { if (method.Enable) { await method.Execute(receiver); } }); });
		Array.ForEach(SendMessage.ToArray(), method =>
		{
			mysBot.MessageReceiver.OfType<SendMessageReceiver>().Subscribe(async (receiver) =>
			{
				if (method.Enable)
				{
					if (receiver.commond == $"/{method.GetType().GetCustomAttribute<SendMessageAttribute>()!.Commond}" || receiver.commond == $"{method.GetType().GetCustomAttribute<SendMessageAttribute>()!.Commond}")
					{
						Logger.Log($"Commond:{method.GetType().GetCustomAttribute<SendMessageAttribute>()!.Commond}");
						await method.Execute(receiver);
					}
					else if (method.GetType().GetCustomAttribute<SendMessageAttribute>()!.Commond == "*") //若填入*则代表接收任何消息
					{
						Logger.Log($"Commond:{method.GetType().GetCustomAttribute<SendMessageAttribute>()!.Commond}");
						await method.Execute(receiver);
					}
				}
			});
		});
		Array.ForEach(CreateRobot.ToArray(), method => { mysBot.MessageReceiver.OfType<CreateRobotReceiver>().Subscribe(async (receiver) => { if (method.Enable) { await method.Execute(receiver); } }); });
		Array.ForEach(DeleteRobot.ToArray(), method => { mysBot.MessageReceiver.OfType<DeleteRobotReceiver>().Subscribe(async (receiver) => { if (method.Enable) { await method.Execute(receiver); } }); });
		Array.ForEach(AddQuickEmoticon.ToArray(), method => { mysBot.MessageReceiver.OfType<AddQuickEmoticonReceiver>().Subscribe(async (receiver) => { if (method.Enable) { await method.Execute(receiver); } }); });
		Array.ForEach(AuditCallback.ToArray(), method => { mysBot.MessageReceiver.OfType<AuditCallbackReceiver>().Subscribe(async (receiver) => { if (method.Enable) { await method.Execute(receiver); } }); });

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
					if (string.IsNullOrEmpty(commond) || string.IsNullOrWhiteSpace(commond))
					{ continue; }
					string?[] commondSplit = commond.Split(" ");
					Logger.Log($"input {commond}");
					Type type = typeof(Commond);
					MethodInfo? methodInfo;
					try
					{
						methodInfo = type.GetMethod(commondSplit[0]!);
					}
					catch (ArgumentException)
					{
						Logger.LogError($"没有此方法！");
						continue;
					}

					if (String.IsNullOrWhiteSpace(commond)) continue;

					try
					{
						methodInfo!.Invoke(null, new object[] { commondSplit });
					}
					catch (ArgumentException e)
					{
						Logger.LogError(e.Message);
						continue;
					}
					catch (NullReferenceException e)
					{
						Logger.LogError(e.Message);
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
	public static void exit(string?[] args)
	{
		Environment.Exit(0);
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
