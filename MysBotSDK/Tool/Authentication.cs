using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MysBotSDK.Tool;

internal static class Authentication
{
	internal static bool Verify(string body, string botSign, string pub_key, string botSecret)
	{
		byte[] sign = Convert.FromBase64String(botSign);
		string combinedData_ = "body=" + UrlEncode(body.TrimEnd('\n')) + "&secret=" + botSecret;
		byte[] signData = Encoding.UTF8.GetBytes(combinedData_);
		try
		{
			using (RSA rsa = RSA.Create())
			{
				rsa.ImportFromPem(pub_key);

				if (rsa.VerifyData(signData, sign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
				{
					return true;
				}
			}
		}
		catch (CryptographicException)
		{
			// Verification failed
		}

		return false;
	}
	static string UrlEncode(string source)
	{
		string encodeUrl = System.Net.WebUtility.UrlEncode(source).Replace("(", "%28").Replace(")", "%29").Replace("!", "%21").Replace("*", "%2A").Replace("%7E", "~").Replace(" ", "+");
		return encodeUrl;
	}

	internal static string HmacSHA256(string secret, string pub_key)
	{
		using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(pub_key.Replace("\r", ""))))
		{
			byte[] rawSecret = Encoding.UTF8.GetBytes(secret);
			byte[] hashBytes = hmac.ComputeHash(rawSecret);
			string botSecret = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
			return botSecret;
		}
	}
}
