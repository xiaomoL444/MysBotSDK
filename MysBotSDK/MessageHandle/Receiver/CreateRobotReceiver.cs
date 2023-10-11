using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{
	public class CreateRobotReceiver : MessageReceiverBase
	{
		public CreateRobot? createRobot { get; set; }
		public CreateRobotReceiver(string message) : base(message)
		{
		}
		public override void Initialize(string message)
		{
			createRobot = JsonConvert.DeserializeObject<CreateRobot>(message)!;
		}
	}
}
