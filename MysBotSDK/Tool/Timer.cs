using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.Tool
{
	public static class Timer
	{
		static string config_path = "./Timer.conf";
		private static Dictionary<string, string> timers_startTime = new Dictionary<string, string>();
		private static List<(string name,System.Timers.ElapsedEventHandler eventHandler, int hour, int minute)> timers_conf = new List<(string,System.Timers.ElapsedEventHandler eventHandler, int hour, int minute)>();
		public static void Register(string name, System.Timers.ElapsedEventHandler eventHandle, int hour, int minute)
		{
			timers_conf.Add((name,eventHandle, hour, minute));
			var file = FileHandle.ReadAsDicString(config_path);
			if (file.ContainsKey(name))
			{
				file[name] = "0";
				timers_startTime = file;
				FileHandle.SaveDicString(config_path, file);
			}

		}
		/// <summary>
		/// 一被引用就开始计时(每三十秒便利一遍计时的事件)
		/// </summary>
		static Timer()
		{
			System.Timers.Timer timer = new System.Timers.Timer(1000 * 30);
			timer.Elapsed += (sender, e) =>
			{
				foreach (var timer in timers_conf)
				{
					if (timers_startTime[timer.name]!= DateTimeOffset.Now.DayOfYear.ToString())
					{
						if (DateTimeOffset.Now.Hour >= timer.hour && DateTimeOffset.Now.Minute >= timer.minute)
						{
							timers_startTime[timer.name] = DateTimeOffset.Now.DayOfYear.ToString();
							FileHandle.SaveDicString(config_path, timers_startTime);
							//执行定时任务
					    	timer.eventHandler.Invoke(sender, e);
							
						}
					}
				}
			};
			timer.Start();
		}
	}
}
