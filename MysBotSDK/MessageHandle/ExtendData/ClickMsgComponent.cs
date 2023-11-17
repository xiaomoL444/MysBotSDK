using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.ExtendData
{
	internal class ClickMsgComponent
	{

		/// <summary>
		/// 大别野 id
		/// </summary>
		public UInt64 villa_id { get; set; }

		/// <summary>
		/// 房间 id
		/// </summary>
		public UInt64 room_id { get; set; }

		/// <summary>
		/// 用户 id
		/// </summary>
		public UInt64 uid { get; set; }

		/// <summary>
		/// 消息 id
		/// </summary>
		public string msg_uid { get; set; }

		/// <summary>
		/// 如果消息从属于机器人，则该字段不为空字符串
		/// </summary>
		public string bot_msg_id { get; set; }

		/// <summary>
		/// 机器人自定义的组件id
		/// </summary>
		public string component_id { get; set; }

		/// <summary>
		/// 如果该组件模板为已创建模板，则template_id不为0
		/// </summary>
		public UInt64 template_id { get; set; }

		/// <summary>
		/// 机器人自定义透传信息
		/// </summary>
		public string extra { get; set; }

	}
}
