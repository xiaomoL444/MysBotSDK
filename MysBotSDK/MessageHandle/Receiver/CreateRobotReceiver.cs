using MysBotSDK.MessageHandle.ExtendData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{
	/// <summary>
	/// 机器人加入大别野事件接收器
	/// </summary>
	public class CreateRobotReceiver : MessageReceiverBase
	{
		/// <summary>
		/// 加入大别野的ID
		/// </summary>
		public UInt64 Villa_ID => createRobot.villa_id;
		internal CreateRobot createRobot { get; set; }
		public CreateRobotReceiver(string message) : base(message)
		{
		}
		internal override void Initialize(string message)
		{
			createRobot = JsonConvert.DeserializeObject<CreateRobot>(message)!;
		}
	}
}
