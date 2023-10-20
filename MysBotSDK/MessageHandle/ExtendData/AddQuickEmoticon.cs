using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.ExtendData
{
	/// <summary>
	/// 用户表情回复
	/// </summary>
	public class AddQuickEmoticon : IExtendData
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
		/// 发送表情的用户 id
		/// </summary>
		public uint uid { get; set; }

		/// <summary>
		/// 表情 id
		/// </summary>
		public int emoticon_id { get; set; }

		/// <summary>
		/// 表情内容
		/// </summary>
		public string? emoticon { get; set; }

		/// <summary>
		/// 被回复的消息 id
		/// </summary>
		public string? msg_uid { get; set; }

		/// <summary>
		/// 如果被回复的消息从属于机器人，则该字段不为空字符串
		/// </summary>
		public string? bot_msg_id { get; set; }

		/// <summary>
		/// 是否是取消表情
		/// </summary>
		public bool is_cancel { get; set; }
	}
}
