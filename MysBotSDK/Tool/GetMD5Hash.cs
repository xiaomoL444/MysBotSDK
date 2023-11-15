using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.Tool
{
	internal static class GetMD5Hash
	{
		public static string GetMD5HashFromFile(string file_path)
		{
			try
			{
				FileStream file = new FileStream(file_path, System.IO.FileMode.Open);
				//MD5 md5 = new MD5CryptoServiceProvider();
				//byte[] retVal = md5.ComputeHash(file);
				byte[] retVal = MD5.HashData(file);
				file.Close();
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < retVal.Length; i++)
				{
					sb.Append(retVal[i].ToString("x2"));
				}
				return sb.ToString();

			}
			catch (Exception ex)
			{
				Logger.LogError("GetMD5HashFromFile() fail,error:" + ex.Message);
				return string.Empty;
			}
		}
	}
}
