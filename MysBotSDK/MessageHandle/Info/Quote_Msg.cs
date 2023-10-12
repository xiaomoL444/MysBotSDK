using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	/// <summary>
	/// 回调消息引用消息的基础信息
	/// </summary>
	public class Quote_Msg
	{
		/// <summary>
		/// 消息摘要，如果是文本消息，则返回消息的文本内容。如果是图片消息，则返回"[图片]"
		/// </summary>
		public string? content { get; set; }
		/// <summary>
		/// 消息 id
		/// </summary>
		public string? msg_uid { get; set; }
		/// <summary>
		/// 如果消息从属于机器人，则该字段不为空字符串
		/// </summary>
		public string? bot_msg_id { get; set; }
		/// <summary>
		/// 发送时间的时间戳 
		/// </summary>
		public long send_at { get; set; }
		/// <summary>
		/// 消息类型，包括"文本"，"图片"，"帖子卡片"等
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))] public Msg_Type msg_type { get; set; }

		public enum Msg_Type
		{
			文本 = 0,
			图片 = 1,
			帖子卡片 = 2
		}
		/// <summary>
		/// 发送者 id（整型）
		/// </summary>
		public uint from_user_id { get; set; }
		/// <summary>
		/// 发送者昵称
		/// </summary>
		public string? from_user_nickname { get; set; }
		/// <summary>
		/// 发送者 id（字符串）可携带机器人发送者的id
		/// </summary>
		public string? from_user_id_str { get; set; }

	}
}
