using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.Tool;

public class FileHandle
{
	public static object locker = 0;
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
			string tmp_str;
			while ((tmp_str = streamReader.ReadLine()) != null)
			{
				try
				{
					var split = tmp_str.Split("=");
					dic[split[0]] = split[1];
				}
				catch
				{
					Logger.LogError("解析失败");
				}

			}
			streamReader.Close();
		}
		return dic;
	}
	public static void SaveDicString(string path, Dictionary<string, string> dic)
	{
		string content = string.Empty;
		for (int i = 0; i < dic.Count; i++)
		{
			content = dic.ElementAt(i).Key + "=" + dic.ElementAt(i).Value;
			if (i != dic.Count - 1)
			{
				content += "\n";
			}
		}
		lock (locker)
		{
			StreamWriter streamWriter = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite));
			streamWriter.Write(content);
			streamWriter.Close();
		}
	}
}