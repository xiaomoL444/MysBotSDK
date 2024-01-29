using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MysBotSDK.Tool
{
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

		static System.Timers.Timer writeToSQLDateTimer = new System.Timers.Timer(1000);//一秒保存一遍

		//启动写入的计时器
		static Logger()
		{
			writeToSQLDateTimer.Elapsed += (sender, e) => { SQLDateSave(); };
			writeToSQLDateTimer.Start();
		}
		/// <summary>
		/// 设置控制台输出哪种等级以上的信息
		/// </summary>
		public static LoggerLevel loggerLevel { get; set; } = LoggerLevel.Log;
		/// <summary>
		/// 输出等级
		/// </summary>
		public enum LoggerLevel
		{
			/// <summary>
			/// 错误信息
			/// </summary>
			Error = 0,

			/// <summary>
			/// 警告信息
			/// </summary>
			Warning = 1,

			/// <summary>
			/// 普通日志信息
			/// </summary>
			Log = 2,

			/// <summary>
			/// 调试信息
			/// </summary>
			Debug = 3,

			/// <summary>
			/// 调用伺服器Api回传的数据
			/// </summary>
			Network = 4,
		}

		/// <summary>
		/// 输出Network信息
		/// </summary>
		/// <param name="mes">输出的信息</param>
		/// <param name="nameAttribute">调用Debug的方法名(为空即可)</param>
		/// <returns>[DEBUG] [From: {nameAttribute}] {time}:{mes}\n</returns>
		public static string Network(string? mes, [System.Runtime.CompilerServices.CallerMemberName] string nameAttribute = "")
		{
			string log = $"[Network] {GREEN}[From: {nameAttribute}]{RESET} {time}:{mes}";

			WriteToSQLData(LoggerLevel.Network, $"[From: {nameAttribute}]{mes}");

			if (loggerLevel >= LoggerLevel.Network)
			{
				Console.WriteLine(log);
			}
			return $"{mes}\n";
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

			WriteToSQLData(LoggerLevel.Debug, $"[From: {nameAttribute}]{mes}");

			if (loggerLevel >= LoggerLevel.Debug)
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

			WriteToSQLData(LoggerLevel.Log, mes!);

			if (loggerLevel >= LoggerLevel.Log)
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

			WriteToSQLData(LoggerLevel.Warning, mes!);

			if (loggerLevel >= LoggerLevel.Warning)
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

			WriteToSQLData(LoggerLevel.Error, $"[Form: {MemberName}] | [CallForm: {FilePath} Line: {LineNumber}] {mes}");

			if (loggerLevel >= LoggerLevel.Error)
			{
				Console.WriteLine(log);
			}
			return $"{mes}\n";
		}

		private static string _lastDate = string.Empty;
		private static SqliteConnection _sqliteConnection = null!;
		private static List<(string time, LoggerLevel loggerLevel, string content)> sqliteCommandQueue = new();
		/// <summary>
		/// 写入日志
		/// </summary>
		/// <param name="loggerLevel"></param>
		/// <param name="content"></param>
		public static void WriteToSQLData(LoggerLevel loggerLevel, string content)
		{
			sqliteCommandQueue.Add(new() { time = time!, loggerLevel = loggerLevel, content = content });
		}
		internal static void SQLDateSave()
		{
			if (_lastDate != date || _sqliteConnection == null)
			{
				_lastDate = date;

				if (_sqliteConnection != null)
				{
					_sqliteConnection.Close();
				}

				var connectString = new SqliteConnectionStringBuilder() { Mode = SqliteOpenMode.ReadWriteCreate, DataSource = $"Log/{date}.db", Cache = SqliteCacheMode.Shared }.ToString();

				if (!Directory.Exists("./Log/"))
				{
					Directory.CreateDirectory("./Log/");
				}

				//File.Create($"./Log/{date}.db").Close();

				_sqliteConnection = new SqliteConnection(connectString);

				_sqliteConnection.Open();

				#region 创建表格

				//若非开启状态，则返回
				if (_sqliteConnection.State != System.Data.ConnectionState.Open) return;

				var createCommand = _sqliteConnection.CreateCommand();

				createCommand.CommandText = @"CREATE TABLE IF NOT EXISTS Log(
TIME	TEXT	NOT NULL,
TYPE	TEXT	NOT NULL,
CONTENT	TEXT	NOT NULL)";

				createCommand.ExecuteNonQuery();
				#endregion
			}
			#region 插入内容
			lock (logLock)
			{
				int nowCommandCount = sqliteCommandQueue.Count;

				using (var transaction = _sqliteConnection.BeginTransaction())
				{
					try
					{
						for (int i = 0; i < nowCommandCount; i++)
						{
							var command = _sqliteConnection!.CreateCommand();

							command.Transaction = transaction;

							command.CommandText = @"INSERT INTO Log (TIME,TYPE,CONTENT)
VALUES ($time, $type, $content )";

							command.Parameters.AddWithValue("$time", sqliteCommandQueue[i].time);
							command.Parameters.AddWithValue("$type", Enum.GetName(sqliteCommandQueue[i].loggerLevel));
							command.Parameters.AddWithValue("$content", sqliteCommandQueue[i].content);

							command.ExecuteNonQuery();
						}
						transaction.Commit();
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						Logger.LogError($"目前执行的SQL语句出现错误{ex.Message}\n{string.Join("\n", sqliteCommandQueue.Take(nowCommandCount))}");
					}

				}
				sqliteCommandQueue.RemoveRange(0, nowCommandCount);//最后依据执行的数量删除执行的数量(方式有新增的没上传)
			}

			#endregion
		}

	}
}