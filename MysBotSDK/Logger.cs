using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK;
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
	public static string Log(string? mes)
	{
		CheckAvaliable();
		lock (logLock)
		{
			StreamWriter sw = new StreamWriter($"./Log/{date}.txt", true);
			sw.WriteLine($"{time}:{mes}");
			sw.Close();
		}
		if (isWindow)
		{
			Colorful.Console.WriteLine($"{time}:{mes}", Color.LightGreen);
		}
		else
		{
			Console.WriteLine($"{time}:{mes}");
		}
		return mes + "\n";
	}
	public static string LogError(string? mes)
	{
		CheckAvaliable();
		lock (logLock)
		{
			StreamWriter sw = new StreamWriter($"./Log/{date}.txt", true);
			sw.WriteLine($"{time}:{mes}");
			sw.Close();
		}
		if (isWindow)
		{
			Colorful.Console.WriteLine($"{time}:{mes}", Color.Red);
		}
		else
		{
			Console.WriteLine($"{time}:{mes}");
		}
		return $"[ERROR]{mes}\n";
	}
	public static string LogWarnning(string? mes)
	{
		CheckAvaliable();
		lock (logLock)
		{
			StreamWriter sw = new StreamWriter($"./Log/{date}.txt", true);
			sw.WriteLine($"{time}:{mes}");
			sw.Close();
		}
		if (isWindow)
		{
			Colorful.Console.WriteLine($"{time}:{mes}", Color.Yellow);
		}
		else
		{
			Console.WriteLine($"{time}:{mes}");
		}
		return $"[WARNING]{mes}\n";
	}
}