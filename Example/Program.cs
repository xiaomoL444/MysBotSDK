using MysBotSDK;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
using System.Reactive.Linq;

namespace Example;

public class ExampleProgram
{
	public async Task Main(string[] args)
	{
		MysBot mysBot = new MysBot()
		{
			callback_Adress = "",//回调地址
			bot_id = "",
			secret = "",
			pub_key = "",
			loggerLevel = Logger.LoggerLevel.Log,
		};
		mysBot.MessageReceiver
			.OfType<SendMessageReceiver>()
			.Subscribe((receiver) =>{

		});
	}
}