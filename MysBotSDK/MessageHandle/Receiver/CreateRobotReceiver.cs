﻿using MysBotSDK.MessageHandle.ExtendData;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
		/// <summary>
		/// bot加入新大别野事件
		/// </summary>
		/// <param name="message"></param>
		public CreateRobotReceiver(string message) : base(message)
		{
			createRobot = GetExtendDataMsg<CreateRobot>(message);
			villa_id = createRobot.villa_id;

			Logger.Debug($"Receive [CreateRobot] Form villa:{villa_id},room:{room_id}");
		}
	}
}
