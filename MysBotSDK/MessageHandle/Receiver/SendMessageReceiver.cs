using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{
	public class SendMessageReceiver : MessageReceiverBase
	{
		public SendMessage? sendMessage { get; set; }
		public List<string> args => sendMessage.args;

		public SendMessageReceiver(string message) : base(message)
		{
		}
		public override void Initialize(string message)
		{
			sendMessage = JsonConvert.DeserializeObject<SendMessage>(message)!;
			var args = sendMessage!.content.content.text.Split(" ").ToList();
			args.RemoveRange(0, 2);
			sendMessage.args = args;
		}
	}
}
