using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{
	public class DeleteRobotReceiver : MessageReceiverBase
	{
		public DeleteRobot? deleteRobot { get; set; }
		public DeleteRobotReceiver(string message) : base(message)
		{
		}
		public override void Initialize(string message)
		{
			deleteRobot = JsonConvert.DeserializeObject<DeleteRobot>(message)!;
		}
	}

}
