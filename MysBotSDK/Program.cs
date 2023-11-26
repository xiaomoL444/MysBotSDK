using Google.Protobuf;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.ExtendData;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using vila_bot;

namespace MysBotSDK;

internal class Program
{
	public static void Main(string[] args)
	{
		string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
		Logger.Log("========================================================");
		Logger.Log($"当前MysSDK版本:{ver}");
	}
}
