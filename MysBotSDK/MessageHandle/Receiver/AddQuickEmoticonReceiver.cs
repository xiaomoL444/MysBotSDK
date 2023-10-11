using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{
	public class AddQuickEmoticonReceiver : MessageReceiverBase
	{
		public AddQuickEmoticon addQuickEmoticon { get; set; }
		public AddQuickEmoticonReceiver(string message) : base(message)
		{
		}
		public override void Initialize(string message)
		{
			addQuickEmoticon = JsonConvert.DeserializeObject<AddQuickEmoticon>(message)!;
		}
	}

}
