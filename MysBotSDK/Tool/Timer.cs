using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.Tool
{
	public static class Timer
	{
		static string config_path = "./Timer.json";
		private static Dictionary<string, string> timers_startTime = new Dictionary<string, string>();
		private static List<(string name, System.Timers.ElapsedEventHandler eventHandler, int hour, int minute)> timers_conf = new List<(string, System.Timers.ElapsedEventHandler eventHandler, int hour, int minute)>();
		/// <summary>
		/// 注册计时事件
		/// </summary>
		/// <param name="name">事件名称</param>
		/// <param name="eventHandle">事件方法</param>
		/// <param name="hour">触发时间在第几小时(不包含第24时)</param>
		/// <param name="minute">触发时间在第几分钟(不包含第60分钟)</param>
		public static void Register(string name, System.Timers.ElapsedEventHandler eventHandle, int hour, int minute)
		{
			Logger.Log($"注册计时事件:{name}");
			if (timers_conf.Any(q => q.name == name))
			{
				Logger.LogWarnning($"已有同样的计时器时间创建{name}");
			}
			timers_conf.Add((name, eventHandle, hour, minute));

			var file = FileHandle.ReadAsDicString(config_path);
			if (!file.ContainsKey(name))
			{
				file[name] = "0";
				FileHandle.SaveDicString(config_path, file);
			}
			timers_startTime = file;

		}
		/// <summary>
		/// 一被引用就开始计时(每三十秒便利一遍计时的事件)
		/// </summary>
		static Timer()
		{
			System.Timers.Timer timer = new System.Timers.Timer(1000 * 1);
			timer.Elapsed += (sender, e) =>
			{
				foreach (var timer in timers_conf)
				{
					if (timers_startTime[timer.name] != DateTimeOffset.Now.DayOfYear.ToString())
					{
						if (DateTimeOffset.Now.Hour == timer.hour && DateTimeOffset.Now.Minute == timer.minute)
						{
							timers_startTime[timer.name] = DateTimeOffset.Now.DayOfYear.ToString();
							FileHandle.SaveDicString(config_path, timers_startTime);
							//执行定时任务
							Logger.Log($"执行事件 {timer.name}");
							timer.eventHandler.Invoke(sender, e);

						}
					}
				}
			};
			timer.Start();
		}
	}
}
