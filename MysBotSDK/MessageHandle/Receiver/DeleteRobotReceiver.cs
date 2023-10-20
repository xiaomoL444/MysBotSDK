﻿using MysBotSDK.MessageHandle.ExtendData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{
	/// <summary>
	/// 大别野删除机器人事件接收器
	/// </summary>
	public class DeleteRobotReceiver : MessageReceiverBase
	{
		/// <summary>
		/// 离开大别野的ID
		/// </summary>
		public UInt64 Villa_ID => deleteRobot.villa_id;
		internal DeleteRobot deleteRobot { get; set; }
		public DeleteRobotReceiver(string message) : base(message)
		{
		}
		internal override void Initialize(string message)
		{
			deleteRobot = JsonConvert.DeserializeObject<DeleteRobot>(message)!;
		}
	}

}
