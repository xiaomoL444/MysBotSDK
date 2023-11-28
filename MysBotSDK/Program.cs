using Google.Protobuf;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.ExtendData;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
using Newtonsoft.Json;

namespace MysBotSDK;

internal class Program
{
	public static void Main(string[] args)
	{
		string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
		Logger.Log("========================================================");
		Logger.Log($"当前MysSDK版本:{ver}");
		while (true)
		{
			Task.Delay(1000);
			Logger.Log($"{GetCurrentTime()}");
		}
	}
	public static UInt64 GetCurrentTime()
	{
		return (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMicroseconds;
	}
}
