using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Modules;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace MysBotSDK;

public static class Program
{
	/// <summary>
	/// Bot实例
	/// </summary>
	internal static MysBot? mysBot;

	/// <summary>
	/// 接收器们
	/// </summary>
	internal static Dictionary<string, List<(string pluginName, IMysSDKBaseModule module)>> receivers { get; set; } = new();

	/// <summary>
	/// Task任务的接收器们
	/// </summary>
	internal static Dictionary<string, List<IMysTaskModule>> taskReceiver { get; set; } = new();
	public static async Task Main(string[] args)
	{
		string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
		Logger.Log("=========================================================");
		Logger.Log($"当前MysSDK版本:{ver}");

		Logger.Log($"加载Bot信息");
		LoadBotConfig();

		Logger.Log($"加载插件信息");
		if (args != null && args.Length != 0 && args[0] != string.Empty)
		{
			Logger.Log("加载自身项目指定命名空间下的插件");
			LoadPluginsInProject(args.ToList());
		}
		else
		{
			LoadPluginsInDirectory();
		}

		Logger.Debug("设置Bot的接收器");
		SetMysBotReceiverHandle();

		await Command();
	}

	/// <summary>
	/// 加载Bot配置信息
	/// </summary>
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

	internal static void SetMysBotReceiverHandle()
	{
		foreach (var receiver in receivers.Keys)
		{
			var receiverType = (EventType)Enum.Parse(typeof(EventType), receiver.Replace("Attribute", string.Empty));
			var receiverName = receiver;
			switch (receiverType)
			{
				case EventType.JoinVilla:
					mysBot.MessageReceiver.OfType<JoinVillaReceiver>().Subscribe(messageReceiver =>
					{
						TryExecuteModules(receivers[receiverName].Select(m => m.module).ToList(), messageReceiver);
					});
					break;
				case EventType.SendMessage:
					mysBot.MessageReceiver.OfType<SendMessageReceiver>().Subscribe(messageReceiver =>
					{
						bool isBlock = false;
						receivers[receiver].ForEach(module =>
						{
							if (module.module.IsEnable && !isBlock)
							{
								if (messageReceiver.commond == $"/{module.module.GetType().GetCustomAttribute<SendMessageAttribute>()!.command}"
								|| messageReceiver.commond == $"{module.module.GetType().GetCustomAttribute<SendMessageAttribute>()!.command}"
								|| module.module.GetType().GetCustomAttribute<SendMessageAttribute>()!.command == "*") //若填入*则代表接收任何消息
								{
									if (module.module.GetType().GetCustomAttribute<SendMessageAttribute>()!.isBlock && module.module.IsEnable)
									{
										isBlock = true;
										Logger.Log($"Commond:{module.module.GetType().GetCustomAttribute<SendMessageAttribute>()!.command}");
									}
									TryExecuteModules(new List<IMysSDKBaseModule>() { module.module }, messageReceiver);

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
					mysBot.MessageReceiver.OfType<CreateRobotReceiver>().Subscribe(messageReceiver => { TryExecuteModules(receivers[receiverName].Select(m => m.module).ToList(), messageReceiver); });
					break;
				case EventType.DeleteRobot:
					mysBot.MessageReceiver.OfType<DeleteRobotReceiver>().Subscribe(messageReceiver => { TryExecuteModules(receivers[receiverName].Select(m => m.module).ToList(), messageReceiver); });
					break;
				case EventType.AddQuickEmoticon:
					mysBot.MessageReceiver.OfType<AddQuickEmoticonReceiver>().Subscribe(messageReceiver => { TryExecuteModules(receivers[receiverName].Select(m => m.module).ToList(), messageReceiver); });
					break;
				case EventType.AuditCallback:
					mysBot.MessageReceiver.OfType<AuditCallbackReceiver>().Subscribe(messageReceiver => { TryExecuteModules(receivers[receiverName].Select(m => m.module).ToList(), messageReceiver); });
					break;
				case EventType.ClickMsgComponent:
					mysBot.MessageReceiver.OfType<ClickMsgComponentReceiver>().Subscribe(messageReceiver => { TryExecuteModules(receivers[receiverName].Select(m => m.module).ToList(), messageReceiver); });
					break;
				default:
					break;
			}
		}
	}

	internal static bool ContainPlugin(string pluginName)
	{
		return taskReceiver.ContainsKey(pluginName) || receivers.Values.Any(list => list.Any(q => q.pluginName == pluginName));
	}

	/// <summary>
	/// 加载单个插件集
	/// </summary>
	/// <param name="assembly">插件集</param>
	internal static void LoadPlugins(Assembly assembly)
	{
		#region 读取程序集并保存起来
		List<(string pluginName, IMysSDKBaseModule module)> mysSDKBaseModules = new List<(string pluginName, IMysSDKBaseModule module)>();//该程序集下所有Module

		List<(string pluginName, IMysSDKBaseModule module)> mysTaskModules = new List<(string pluginName, IMysSDKBaseModule module)>();//该程序集下所有TaskModule

		try
		{
			Logger.Debug($"读取插件集{assembly.GetName().Name}");

			if (ContainPlugin(assembly.GetName().Name))
			{
				Logger.LogError($"插件 {assembly.GetName().Name} 已存在");
				return;
			}

			var rawModules = assembly.GetTypes().Where(x => x.GetInterfaces().Any(i => i == typeof(IMysSDKBaseModule))).ToList();
			if (rawModules.Count == 0) return;
			mysSDKBaseModules.AddRange(rawModules.Where(q => q.GetInterfaces().Any(i => i == typeof(IMysSDKBaseModule))).Select(Activator.CreateInstance).Select(m => { return (assembly.GetName().Name, m as IMysSDKBaseModule); })!);
		}
		catch (Exception)
		{
			Logger.LogError($"读取程序集{assembly.FullName}错误");
		}

		//输出所有加载成功的方法
		mysSDKBaseModules.ForEach((module) =>
		{
			Logger.Debug($"搜索到方法{string.Join("", module.module.GetType().CustomAttributes)}{module.module.GetType().Name}");

			if (module.module is IMysTaskModule)
			{
				//Logger.Debug($"搜索到Task方法 {module.module.GetType().Name}");
				mysTaskModules.Add(module);
				if (!taskReceiver.ContainsKey(module.pluginName))
				{
					taskReceiver[module.pluginName] = new();
				}
				taskReceiver[module.pluginName].Add((IMysTaskModule)module.module);
			}
		});
		#endregion

		#region Task模块执行

		mysTaskModules.ForEach(module =>
		{
			Logger.Log($"搜索并执行Task方法{module.module.GetType().Name}");
			if (!module.module.IsEnable) return;

			Task.Run(() =>
			{
				((IMysTaskModule)module.module).Start();
			});
		});

		#endregion

		#region 加入订阅receivers

		//获取所有接收器方法
		var allReceiverType = GetSubClassNames(typeof(ExtendDataAttribute));

		//载入插件
		if (mysSDKBaseModules.Count > 0)
		{
			allReceiverType.ForEach(type =>
			{
				if (!receivers.ContainsKey(type.name))
				{
					receivers[type.name] = new();
				}

				receivers[type.name].AddRange(new List<(string pluginName, IMysSDKBaseModule module)>(mysSDKBaseModules.Where(q =>
				{
					var isEqual = q.module.GetType().GetCustomAttribute(type.type) != null;
					if (isEqual)
					{
						Logger.Log($"载入的方法[{type.name.Replace("Attribute", string.Empty)}] {(type.type == typeof(SendMessageAttribute) ? $"[/{q.module.GetType().GetCustomAttribute<SendMessageAttribute>().command.TrimStart('/')}]" : "")} {q.module.GetType().Name} ");
					}
					return isEqual;
				})));
			});
		}
		#region 按照优先级排列模块

		foreach (var detailReceiver in receivers)
		{
			receivers[detailReceiver.Key] = detailReceiver.Value.OrderByDescending(o => o.module.GetType().GetCustomAttribute<ExtendDataAttribute>().priority).ToList();
		}
		#endregion

		#endregion

	}


	/// <summary>
	/// 加载本地插件(./Plugins)
	/// </summary>
	/// <param name="isLoadSelfPlugins">是否要加载本地项目插件(开发插件时开启)</param>
	/// <param name="args">命名空间名</param>
	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void LoadPluginsInDirectory(List<string> args = null)
	{
		#region 通过本地路径加载程序集

		AssemblyLoadContext assemblyLoadContext = new AssemblyLoadContext("Plugins", true);

		WeakReference weakReference = new WeakReference(assemblyLoadContext.Assemblies, trackResurrection: true);

		if (!Directory.Exists("Plugins"))
		{
			Directory.CreateDirectory("Plugins");
			Logger.LogWarnning("不存在路径./Plugins/，创建文件夹");
		}
		var assemblies_path = args == null ? Directory.EnumerateFiles("./Plugins", "*.dll").ToList() : args.Select(arg => $"./Plugins/{arg}.dll").ToList();

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

		#region 通过程序集加载插件方法

		foreach (var assembly in assemblyLoadContext.Assemblies)
		{
			LoadPlugins(assembly);
		}

		Task.Run(() =>
		{
			assemblyLoadContext.Unload();

			//while (true);

			for (int i = 0; weakReference.IsAlive && (i < 10); i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			if (weakReference.IsAlive)
			{
				Logger.LogError($"程序集卸载失败，请检查插件并重启程序");
				return;
			}
			Logger.Debug("assembly卸载成功");
			return;
		});

		return;
		#endregion
	}
	/// <summary>
	/// 加载插件项目内指定命名空间下的module
	/// </summary>
	/// <param name="namespaces"></param>
	internal static void LoadPluginsInProject(List<string> namespaces)
	{
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetTypes().Any(t => namespaces.Any(n => n == t.Namespace))))
		{
			LoadPlugins(assembly);
		}

	}

	/// <summary>
	/// 卸载所有插件
	/// </summary>
	internal static void UnloadPlugins()
	{
		Logger.Log("清空所有方法");
		//清空接收器的所有方法
		foreach (var item in receivers.Keys)
		{
			receivers[item] = new();
		}
		//执行task.Unload()操作
		foreach (var item in taskReceiver.Values)
		{
			item.ForEach(q => { q.Unload(); Logger.LogWarnning($"{q.GetType().Name}执行Task.Unload()操作"); });
		}
	}
	/// <summary>
	/// 根据插件名卸载插件
	/// </summary>
	/// <param name="pluginName"></param>
	internal static void UnloadPlugins(string pluginName)
	{
		if (!ContainPlugin(pluginName))
		{
			Logger.LogError($"插件 {pluginName} 不存在");
			return;
		}
		var newReceivers = new Dictionary<string, List<(string pluginName, IMysSDKBaseModule module)>>(receivers.ToDictionary(q => q.Key, p => new List<(string pluginName, IMysSDKBaseModule module)>(p.Value)));

		if (taskReceiver.ContainsKey(pluginName))
		{
			taskReceiver[pluginName].ForEach(q => { q.Unload(); Logger.LogWarnning($"{q.GetType().Name}执行Task.Unload()操作"); });
			taskReceiver.Remove(pluginName);//移除task任务
		}

		foreach (var item in receivers)
		{
			foreach (var plugins in item.Value)
			{
				if (plugins.pluginName == pluginName)
				{
					Logger.LogWarnning($"卸载方法{plugins.module.GetType().Name}");
					newReceivers[item.Key].Remove(newReceivers[item.Key].First(q => q.pluginName == pluginName));
				}
			}
		}
		receivers = newReceivers;
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
					Logger.LogError($"执行指令错误 \n错误信息: {ex.Message} \n{ex.StackTrace}");
				}
			}
		});
	}

	/// <summary>
	/// 控制台输入命令
	/// </summary>
	/// <returns></returns>
	static async Task Command()
	{
		await Task.Run(() =>
		{
			while (true)
			{
				string? command = Console.ReadLine();
				if (string.IsNullOrEmpty(command) || string.IsNullOrWhiteSpace(command))
				{ continue; }
				string?[] commandSplit = command.Split(" ");
				Logger.Log($"input {command}");
				Type type = typeof(Command);
				MethodInfo? methodInfo;
				try
				{
					methodInfo = type.GetMethod(commandSplit[0]!);
				}
				catch (ArgumentException)
				{
					Logger.LogError($"没有此方法！");
					continue;
				}

				if (String.IsNullOrWhiteSpace(command)) continue;

				try
				{
					methodInfo!.Invoke(null, new object[] { commandSplit });
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
		var assembly = parentType.Assembly;//获取当前父类所在的程序集
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

/// <summary>
/// 控制台可输入命令
/// </summary>
static class Command
{
	[CommandDescribe("help\t--帮助列表")]
	public static void help(string?[] args)
	{
		Type type = typeof(Command);
		Logger.Log("========================");

		string commands = "commond List :";

		foreach (var method in type.GetMethods())
		{
			if (method.Name == "GetType" || method.Name == "ToString" || method.Name == "Equals" || method.Name == "GetHashCode") continue;

			commands += $"\n{method.GetCustomAttribute<CommandDescribeAttribute>()?.command}";


		}

		Logger.Log(commands);

		Logger.Log("========================");
	}
	[CommandDescribe("exit\t--退出程式")]
	public static void exit(string?[] args)
	{
		if (Program.mysBot != null)
		{
			Program.mysBot?.Dispose();
		}
		Environment.Exit(0);
	}

	[CommandDescribe("unloadPlugins\t--卸载所有插件\nunloadPlugins [pluginsName] [pluginsName] ...\t--卸载单个插件")]
	public static void unloadPlugins(string?[] args)
	{
		if (args?.Length <= 1)
		{
			Program.UnloadPlugins();
		}
		else
		{
			foreach (var pluginName in args!.Where(arg => arg != "unloadPlugins"))
			{

				Program.UnloadPlugins(pluginName!);
			}
		}
	}
	[CommandDescribe("loadPlugins\t--加载所有插件\nloadPlugins [pluginsName] [pluginsName] ...\t--加载单个插件")]
	public static void loadPlugins(string?[] args)
	{
		if (args?.Length <= 1)
		{
			Program.LoadPluginsInDirectory();
		}
		else
		{
			Program.LoadPluginsInDirectory(args.Where(arg => arg != "loadPlugins").ToList()!);
		}
	}
	[CommandDescribe("reloadPlugins\t--重载所有方法")]
	public static async void reloadPlugins(string?[] args)
	{
		Program.UnloadPlugins();
		Program.LoadPluginsInDirectory();
	}
	[CommandDescribe("listPlugins\t--显示已加载的插件集")]
	public static async void listPlugins(string?[] args)
	{
		var pluginsName = new List<string>();
		foreach (var item in Program.receivers.Values)
		{
			foreach (var plugins in item)
			{
				if (!pluginsName.Any(p => p == plugins.pluginName))
				{
					pluginsName.Add(plugins.pluginName);
				}
			}
		}
		int index = 0;
		pluginsName.ForEach(p =>
		{
			Logger.Log($"{index++}: {p}");
		});
	}
}
class CommandDescribeAttribute : Attribute
{
	public string command { get; set; } = string.Empty;
	public CommandDescribeAttribute(string commands)
	{
		this.command = commands;
	}
}