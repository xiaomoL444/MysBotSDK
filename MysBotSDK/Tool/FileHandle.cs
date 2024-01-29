using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.Tool
{

	public class FileHandle
	{
		private static object locker = new();

		/// <summary>
		/// 读取配置文件
		/// </summary>
		/// <param name="path">文件路径</param>
		/// <returns>返回字典类型配置文件</returns>
		public static Dictionary<string, string> ReadAsDicString(string path)
		{
			Dictionary<string, string> dic = new Dictionary<string, string>();
			lock (locker)
			{
				StreamReader streamReader = new StreamReader(new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite));
				try
				{
					dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(streamReader.ReadToEnd())!;
				}
				catch
				{
					Logger.LogError($"解析{path}失败");
				}

				streamReader.Close();
			}
			if (dic == null)
			{
				dic = new Dictionary<string, string>();
			}
			return dic;
		}

		/// <summary>
		/// 储存文件
		/// </summary>
		/// <param name="path">路径</param>
		/// <param name="dic">需要储存的字典</param>
		/// <returns>作为参数的dic字典</returns>
		public static Dictionary<string, string> SaveDicString(string path, Dictionary<string, string> dic)
		{
			lock (locker)
			{
				if (!File.Exists(path))
				{
					File.Create(path).Close();
				}
				StreamWriter streamWriter = new StreamWriter(new FileStream(path, FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite));
				streamWriter.Write(JsonConvert.SerializeObject(dic));
				streamWriter.Close();
			}
			return dic;
		}
	}
}