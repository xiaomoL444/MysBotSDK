using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.Tool;

public class FileHandle
{
	private static object locker = 0;
	/// <summary>
	/// 读取文件，以 string = string 读取入字典
	/// </summary>
	/// <param name="path"></param>
	public static Dictionary<string, string> ReadAsDicString(string path)
	{
		Dictionary<string, string> dic = new Dictionary<string, string>();
		lock (locker)
		{
			StreamReader streamReader = new StreamReader(new FileStream(path, FileMode.OpenOrCreate));
			try
			{
				dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(streamReader.ReadToEnd())!;
			}
			catch
			{
				Logger.LogError("解析失败");
			}

			streamReader.Close();
		}
		if (dic == null)
		{
			dic = new Dictionary<string, string>();
		}
		return dic;
	}
	public static Dictionary<string, string> SaveDicString(string path, Dictionary<string, string> dic)
	{
		lock (locker)
		{
			StreamWriter streamWriter = new StreamWriter(new FileStream(path, FileMode.Truncate, FileAccess.ReadWrite));
			streamWriter.Write(JsonConvert.SerializeObject(dic));
			streamWriter.Close();
		}
		return dic;
	}
}