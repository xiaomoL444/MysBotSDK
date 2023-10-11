using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{
	public class JoinVillaReceiver : MessageReceiverBase
	{
		public JoinVilla? joinVilla { get; set; }
		public JoinVillaReceiver(string message) : base(message)
		{

		}
		public override void Initialize(string message)
		{
			joinVilla = JsonConvert.DeserializeObject<JoinVilla>(message)!;
		}
	}
}
