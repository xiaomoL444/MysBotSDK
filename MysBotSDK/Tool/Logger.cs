﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.Tool;
/// <summary>
/// Log等级 Error>Warning>Log>Debug
/// </summary>
public static class Logger
{
	const string RESET = "\x1B[0m";
	const string RED = "\x1B[91m";
	const string GREEN = "\x1B[92m";
	const string YELLOW = "\x1B[93m";
	const string BLUE = "\x1B[94m";
	static object logLock = new();
	static string date
	{
		get
		{
			string _date = $"{DateTimeOffset.UtcNow.LocalDateTime.Year}-{DateTimeOffset.UtcNow.LocalDateTime.Month}-{DateTimeOffset.UtcNow.LocalDateTime.Day}";
			if (!Directory.Exists("./Log/"))
			{
				Directory.CreateDirectory("./Log/");
			}
			if (!File.Exists($"./Log/{_date}.txt"))
			{
				File.Create($"./Log/{_date}.txt").Close();
			}

			return _date;
		}
	}
	static string? time
	{
		get
		{
			return DateTimeOffset.Now.ToString("G");
		}
	}
	static bool isWindow
	{
		get
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// Windows 相关逻辑
				return true;
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				// Linux 相关逻辑
				return false;
			}
			return false;
		}
	}

	/// <summary>
	/// 设置控制台输出哪种等级以上的信息
	/// </summary>
	public static LoggerLevel loggerLevel { get; set; } = LoggerLevel.Log;
	public enum LoggerLevel
	{
		Error = 0,
		Warning = 1,
		Log = 2,
		Debug = 3,
	}

	/// <summary>
	/// 输出Debug信息
	/// </summary>
	/// <param name="mes">输出的信息</param>
	/// <param name="nameAttribute">调用Debug的方法名(为空即可)</param>
	/// <returns>[DEBUG] [From: {nameAttribute}] {time}:{mes}\n</returns>
	public static string Debug(string? mes, [System.Runtime.CompilerServices.CallerMemberName] string nameAttribute = "")
	{
		string log = $"{BLUE}[DEBUG]{RESET} {GREEN}[From: {nameAttribute}]{RESET} {BLUE}{time}:{mes}{RESET}";
		lock (logLock)
		{
			StreamWriter sw = new StreamWriter($"./Log/{date}.txt", true);
			sw.WriteLine(log);
			sw.Close();
		}
		if ((int)loggerLevel >= 3)
		{
			Console.WriteLine(log);
		}
		return $"{mes}\n";
	}

	/// <summary>
	/// 输出Log信息
	/// </summary>
	/// <param name="mes">输出的信息</param>
	/// <returns>[Log] {time}:{mes}\n</returns>
	public static string Log(string? mes)
	{
		string log = $"{GREEN}[Log] {time}:{mes}{RESET}";

		lock (logLock)
		{
			StreamWriter sw = new StreamWriter($"./Log/{date}.txt", true);
			sw.WriteLine(log);
			sw.Close();
		}
		if ((int)loggerLevel >= 2)
		{
			Console.WriteLine(log);
		}
		return $"{mes}\n";
	}

	/// <summary>
	/// 输出Warning信息
	/// </summary>
	/// <param name="mes">输出的信息</param>
	/// <returns>[WARNING]{time}:{mes}\n</returns>
	public static string LogWarnning(string? mes)
	{
		string log = $"{YELLOW}[WARNING] {time}:{mes}{RESET}";
		lock (logLock)
		{
			StreamWriter sw = new StreamWriter($"./Log/{date}.txt", true);
			sw.WriteLine(log);
			sw.Close();
		}
		if ((int)loggerLevel >= 1)
		{
			Console.WriteLine(log);
		}
		return $"{mes}\n";
	}

	/// <summary>
	/// 输出的Error信息
	/// </summary>
	/// <param name="mes">输出的信息</param>
	/// <param name="MemberName">调用LogError的方法名(为空即可)</param>
	/// <param name="FilePath">调用LogError的代码路径(为空即可)</param>
	/// <param name="LineNumber">调用LogError的代码行数(为空即可)</param>
	/// <returns>[ERROR] [Form: {MemberName}] [CallForm: {FilePath} Line: {LineNumber}] {time}:{mes}\n</returns>
	public static string LogError(string? mes, [System.Runtime.CompilerServices.CallerMemberName] string MemberName = "", [System.Runtime.CompilerServices.CallerFilePath] string FilePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int LineNumber = 0)
	{
		string log = $"{RED}[ERROR]{RESET} {GREEN}[Form: {MemberName}] | [CallForm: {FilePath} Line: {LineNumber}]{RESET}{RED} {time}:{mes}{RESET}";
		lock (logLock)
		{
			StreamWriter sw = new StreamWriter($"./Log/{date}.txt", true);
			sw.WriteLine(log);
			sw.Close();
		}
		if ((int)loggerLevel >= 0)
		{
			Console.WriteLine(log);
		}
		return $"{mes}\n";
	}
}