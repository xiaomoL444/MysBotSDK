using MysBotSDK;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
using System.Reactive.Linq;

namespace Example;

public class ExampleProgram
{
	public static void Main(string[] args)
	{
		MysBot mysBot = new MysBot()
		{
			http_callback_Address = "",//回调地址
			bot_id = "",
			secret = "",
			pub_key = "",
			loggerLevel = Logger.LoggerLevel.Log,
		};
		mysBot.MessageReceiver
			.OfType<SendMessageReceiver>()
			.Subscribe(async (receiver) =>
			{
				var messageChain = new MessageChain()
				.Text("123");
				await receiver.SendText(messageChain);
				//await MessageSender.SendText(receiver.Villa_ID, receiver.Room_ID, messageChain);
				//await MessageSender.SendText(mysBot,receiver.Villa_ID, receiver.Room_ID, messageChain);
			});
	}
}