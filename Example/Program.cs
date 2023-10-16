using MysBotSDK;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
using System.Reactive.Linq;

namespace Example;

public class ExampleProgram
{
	public static async Task Main(string[] args)
	{
		MysBot mysBot = new MysBot()
		{
			http_callback_Adress = "",//回调地址
			bot_id = "",
			secret = "",
			pub_key = "",
			loggerLevel = Logger.LoggerLevel.Log,
		};
		mysBot.MessageReceiver
			.OfType<SendMessageReceiver>()
			.Subscribe((receiver) =>
			{
				var messageChain = new MessageChain()
				.Text("123");
				MessageSender.SendText(receiver.Villa_ID, receiver.Room_ID, messageChain);
			});
	}
}