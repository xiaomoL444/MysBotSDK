using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.ExtendData;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Modules;
using MysBotSDK.Tool;
using Newtonsoft;
using Newtonsoft.Json;
using System;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace MysBotSDK;

static class Program
{
	internal static MysBot? mysBot;

	internal static bool isUnload { get; set; } = false;

	internal static WeakReference? weakReference { get; set; }
	static async Task Main(string[] args)
	{
		string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
		Logger.Log("=========================================================");
		Logger.Log($"当前MysSDK版本:{ver}");

		Logger.Log($"加载Bot信息");
		LoadBotConfig();

		Logger.Log($"加载插件信息");
		LoadPlugins();



		await Command();
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
			PressAndExitProgram();
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
			PressAndExitProgram();
		}
	}
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LoadPlugins()
	{

		#region 加载插件路径
		List<IMysSDKBaseModule> plugins = new List<IMysSDKBaseModule>();

		AssemblyLoadContext assemblyLoadContext = new AssemblyLoadContext("Plugins", true);
		weakReference = new WeakReference(assemblyLoadContext.Assemblies, trackResurrection: true);

		if (!Directory.Exists("Plugins"))
		{
			Directory.CreateDirectory("Plugins");
			Logger.LogWarnning("不存在路径./Plugins/，创建文件夹");
		}
		var assemblies_path = Directory.EnumerateFiles("./Plugins", "*.dll").ToList();

		assemblies_path.ForEach((path) =>
		{
			try
			{
				var steam = new FileStream(path, FileMode.Open);
				assemblyLoadContext.LoadFromStream(steam);
				steam.Close();
			}
			catch (Exception)
			{
				Logger.LogError($"加载插件路径[{path}]失败");
			}
		});

		#endregion

		//获取所有接收器方法
		var allReceiverType = GetSubClassNames(typeof(ExtendDataAttribute));

		#region 加载插件方法

		List<IMysSDKBaseModule> mysSDKBaseModules = new List<IMysSDKBaseModule>();

		List<IMysTaskModule> mysTaskModules = new List<IMysTaskModule>();

		foreach (var assembly in assemblyLoadContext.Assemblies)
		{
			try
			{
				Logger.Debug($"读取插件集{assembly.GetName()}");
				var rawModules = assembly.GetTypes().Where(x => x.GetInterfaces().Any(i => i == typeof(IMysSDKBaseModule))).ToList();
				if (rawModules.Count == 0) continue;
				mysSDKBaseModules.AddRange(rawModules.Where(q => q.GetInterfaces().Any(i => i == typeof(IMysSDKBaseModule))).Select(Activator.CreateInstance).Select(m => (m as IMysSDKBaseModule)!));
			}
			catch (Exception)
			{
				Logger.LogError($"读取程序集{assembly.FullName}错误");
			}

		}

		//输出所有加载成功的方法
		mysSDKBaseModules.ForEach((module) =>
		{
			Logger.Debug($"搜索到方法[{string.Join("", module.GetType().CustomAttributes)}]{module.GetType().Name}");

			if (module is IMysTaskModule)
			{
				Logger.Debug($"搜索到Task方法 {module.GetType().Name}");
				mysTaskModules.Add((IMysTaskModule)module);
			}
		});

		#endregion



		#region Task模块执行

		mysTaskModules.ForEach(module =>
		{
			Logger.Log($"搜索并执行Task方法{module.GetType().Name}");
			if (!module.IsEnable) return;
			Task.Run(async () =>
			{
				await module.Start();
			});
		});

		#endregion

		#region 加入订阅器
		Dictionary<string, List<IMysSDKBaseModule>> receivers = new();

		//载入插件
		if (mysSDKBaseModules.Count > 0)
		{
			allReceiverType.ForEach(type =>
			{
				receivers[type.name] = new List<IMysSDKBaseModule>(mysSDKBaseModules.Where(q =>
				{
					var isEqual = q.GetType().GetCustomAttribute(type.type) != null;
					if (isEqual)
					{
						Logger.Log($"载入的方法[{type.name.Replace("Attribute", string.Empty)}] {q.GetType().Name} ");
					}
					return isEqual;
				}));
			});
		}

		#region 按照优先级排列模块

		foreach (var detailReceiver in receivers)
		{
			receivers[detailReceiver.Key] = detailReceiver.Value.OrderByDescending(o => o.GetType().GetCustomAttribute<ExtendDataAttribute>().priority).ToList();
		}
		#endregion

		foreach (var receiver in receivers.Keys)
		{
			var receiverType = (EventType)Enum.Parse(typeof(EventType), receiver.Replace("Attribute", string.Empty));
			var moduleList = receivers[receiver];
			switch (receiverType)
			{
				case EventType.JoinVilla:
					mysBot.MessageReceiver.OfType<JoinVillaReceiver>().Subscribe(messageReceiver =>
					{
						TryExecuteModules(moduleList, messageReceiver);
					});
					break;
				case EventType.SendMessage:
					mysBot.MessageReceiver.OfType<SendMessageReceiver>().Subscribe(messageReceiver =>
					{
						bool isBlock = false;
						receivers[receiver].ForEach(module =>
						{
							if (module.IsEnable && !isBlock)
							{
								if (messageReceiver.commond == $"/{module.GetType().GetCustomAttribute<SendMessageAttribute>()!.Commond}"
								|| messageReceiver.commond == $"{module.GetType().GetCustomAttribute<SendMessageAttribute>()!.Commond}"
								|| module.GetType().GetCustomAttribute<SendMessageAttribute>()!.Commond == "*") //若填入*则代表接收任何消息
								{
									if (module.GetType().GetCustomAttribute<SendMessageAttribute>()!.isBlock && module.IsEnable)
									{
										isBlock = true;
										Logger.Log($"Commond:{module.GetType().GetCustomAttribute<SendMessageAttribute>()!.Commond}");
									}
									TryExecuteModules(new List<IMysSDKBaseModule>() { module }, messageReceiver);

								}
								//else if ()
								//{
								//	Logger.Log($"Commond:{module.GetType().GetCustomAttribute<SendMessageAttribute>()!.Commond}");
								//	await ((IMysReceiverModule)module).Execute(messageReceiver);
								//}
							}
						});
					});
					break;
				case EventType.CreateRobot:
					mysBot.MessageReceiver.OfType<CreateRobotReceiver>().Subscribe(messageReceiver => { TryExecuteModules(moduleList, messageReceiver); });
					break;
				case EventType.DeleteRobot:
					mysBot.MessageReceiver.OfType<DeleteRobotReceiver>().Subscribe(messageReceiver => { TryExecuteModules(moduleList, messageReceiver); });
					break;
				case EventType.AddQuickEmoticon:
					mysBot.MessageReceiver.OfType<AddQuickEmoticonReceiver>().Subscribe(messageReceiver => { TryExecuteModules(moduleList, messageReceiver); });
					break;
				case EventType.AuditCallback:
					mysBot.MessageReceiver.OfType<AuditCallbackReceiver>().Subscribe(messageReceiver => { TryExecuteModules(moduleList, messageReceiver); });
					break;
				case EventType.ClickMsgComponent:
					mysBot.MessageReceiver.OfType<ClickMsgComponentReceiver>().Subscribe(messageReceiver => { TryExecuteModules(moduleList, messageReceiver); });
					break;
				default:
					break;
			}
		}
		#endregion

		#region 卸载插件
		_ = Task.Run(() =>
		{
			while (true)
			{
				if (!isUnload)
				{
					continue;
				}
				isUnload = false;
				Logger.LogWarnning("执行卸载插件操作");
				assemblyLoadContext.Unloading += (context) => { Logger.Log($"卸载插件中"); };

				//删除引用

				mysBot.ClearHandle();
				//receivers.Clear();
				//mysSDKBaseModules.Clear();
				//plugins.Clear();

				//Task任务停止
				mysTaskModules.ForEach((module) =>
				{
					if (!module.IsEnable) return;

					Logger.Debug($"停止Task方法 {module.GetType().Name}");
					module.Unload();

				});

				//卸载操作

				assemblyLoadContext.Unload();

				for (int i = 0; weakReference.IsAlive && (i < 10); i++)
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
				}

				if (weakReference.IsAlive)
				{
					Logger.LogError("插件卸载失败，请检查插件并重启程序");
					return;
				}
				Logger.Log("插件卸载成功");
				return;
			}

		});
		#endregion
	}

	internal static void UnloadPlugins()
	{
		isUnload = true;
	}

	/// <summary>
	/// 尝试执行指令
	/// </summary>
	internal static void TryExecuteModules(List<IMysSDKBaseModule> modules, MessageReceiverBase messageReceiver)
	{
		modules.ForEach(async module =>
		{
			if (module.IsEnable)
			{
				try
				{
					await ((IMysReceiverModule)module).Execute(messageReceiver);
				}
				catch (Exception ex)
				{
					Logger.LogError($"执行指令错误 {ex.Message} \n{ex.StackTrace}");
				}
			}
		});
	}

	static async Task Command()
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

	/// <summary>
	/// 点击任意键并退出程序
	/// </summary>
	public static void PressAndExitProgram()
	{
		Console.ReadKey();
		Environment.Exit(0);
	}

	/// <summary>
	/// 获取Bot配置文件某个键值
	/// </summary>
	/// <param name="key">键</param>
	/// <returns></returns>
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

	/// <summary>
	/// 获取现在准确的UTC时间
	/// </summary>
	/// <returns></returns>
	static UInt64 GetCurrentTime()
	{
		return (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMicroseconds;
	}

	/// <summary>
	/// 获取一个类在其所在的程序集中的所有子类
	/// </summary>
	/// <param name="parentType">给定的类型</param>
	/// <returns>所有子类的名称</returns>
	static List<(Type type, string name)> GetSubClassNames(Type parentType)
	{
		var subTypeList = new List<Type>();
		var assembly = parentType.Assembly;//获取当前父类所在的程序集``
		var assemblyAllTypes = assembly.GetTypes();//获取该程序集中的所有类型
		foreach (var itemType in assemblyAllTypes)//遍历所有类型进行查找
		{
			var baseType = itemType.BaseType;//获取元素类型的基类
			if (baseType != null)//如果有基类
			{
				if (baseType.Name == parentType.Name)//如果基类就是给定的父类
				{
					subTypeList.Add(itemType);//加入子类表中
				}
			}
		}
		return subTypeList.Select(item => (item, item.Name)).ToList();//获取所有子类类型的名称
	}
}
static class Commond
{
	public static void help(string?[] args)
	{
		Type type = typeof(Commond);
		Logger.Log("========================");
		foreach (var method in type.GetMethods())
		{
			if (method.Name == "GetType" || method.Name == "ToString" || method.Name == "Equals" || method.Name == "GetHashCode") continue;
			Logger.Log(method.Name);
		}
		Logger.Log("========================");
	}
	public static void exit(string?[] args)
	{
		if (Program.mysBot != null)
		{
			Program.mysBot?.Dispose();
		}
		Environment.Exit(0);
	}
	public static void unloadPlugins(string?[] args)
	{
		Program.UnloadPlugins();
	}
	public static void loadPlugins(string?[] args)
	{
		Program.LoadPlugins();
	}
	public static async void reloadPlugins(string?[] args)
	{
		if (Program.weakReference != null && Program.weakReference.IsAlive)
		{
			unloadPlugins(null!);
			while (Program.weakReference!.IsAlive)
			{
				await Task.Delay(100);
			}

		}
		loadPlugins(null!);
	}
}