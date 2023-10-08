﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MysBotSDK;
/// <summary>
/// Log等级 Error>Warning>Log>Debug
/// </summary>
public static class Logger
{
	static object logLock = new object();
	static string date = "";
	static string? time
	{
		get
		{
			return DateTimeOffset.Now.ToString("");
		}
	}
	static bool isWindow;
	public static LoggerLevel loggerLevel { get; set; } = LoggerLevel.Log;//设置控制台输出哪种等级以上的信息
	public enum LoggerLevel
	{
		Error = 0,
		Warning = 1,
		Log = 2,
		Debug = 3
	}

	public static void CheckAvaliable()
	{
		if (date == "")
		{
			date = $"{DateTimeOffset.UtcNow.LocalDateTime.Year}-{DateTimeOffset.UtcNow.LocalDateTime.Month}-{DateTimeOffset.UtcNow.LocalDateTime.Day}";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// Windows 相关逻辑
				isWindow = true;
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				// Linux 相关逻辑
				isWindow = false;
			}
			Log("=========================================================");

		}
		if (!Directory.Exists("./Log/"))
		{
			Directory.CreateDirectory("./Log/");
		}
		if (!File.Exists($"./Log/{date}.txt"))
		{
			File.Create($"./Log/{date}.txt").Close();
		}
	}
	public static string Debug(string? mes)
	{
		CheckAvaliable();
		lock (logLock)
		{
			StreamWriter sw = new StreamWriter($"./Log/{date}.txt", true);
			sw.WriteLine($"[DEBUG]{time}:{mes}");
			sw.Close();
		}
		if ((int)loggerLevel >= 3)
		{
			if (isWindow)
			{
				Colorful.Console.WriteLine($"[DEBUG]{time}:{mes}", Color.LightSkyBlue);
			}
			else
			{
				Console.WriteLine($"[DEBUG]{time}:{mes}");
			}
		}
		return "[DEBUG]" + mes + "\n";
	}
	public static string Log(string? mes)
	{
		CheckAvaliable();
		lock (logLock)
		{
			StreamWriter sw = new StreamWriter($"./Log/{date}.txt", true);
			sw.WriteLine($"{time}:{mes}");
			sw.Close();
		}
		if ((int)loggerLevel >= 2)
		{
			if (isWindow)
			{
				Colorful.Console.WriteLine($"{time}:{mes}", Color.LightGreen);
			}
			else
			{
				Console.WriteLine($"{time}:{mes}");
			}
		}
		return mes + "\n";
	}
	public static string LogWarnning(string? mes)
	{
		CheckAvaliable();
		lock (logLock)
		{
			StreamWriter sw = new StreamWriter($"./Log/{date}.txt", true);
			sw.WriteLine($"[WARNING]{time}:{mes}");
			sw.Close();
		}
		if ((int)loggerLevel >= 1)
		{
			if (isWindow)
			{
				Colorful.Console.WriteLine($"[WARNING]{time}:{mes}", Color.Yellow);
			}
			else
			{
				Console.WriteLine($"[WARNING]{time}:{mes}");
			}
		}
		return $"[WARNING]{mes}\n";
	}
	public static string LogError(string? mes)
	{
		CheckAvaliable();
		lock (logLock)
		{
			StreamWriter sw = new StreamWriter($"./Log/{date}.txt", true);
			sw.WriteLine($"[ERROR]{time}:{mes}");
			sw.Close();
		}
		if ((int)loggerLevel >= 0)
		{
			if (isWindow)
			{
				Colorful.Console.WriteLine($"[ERROR]{time}:{mes}", Color.Red);
			}
			else
			{
				Console.WriteLine($"[ERROR]{time}:{mes}");
			}
		}
		return $"[ERROR]{mes}\n";
	}
}