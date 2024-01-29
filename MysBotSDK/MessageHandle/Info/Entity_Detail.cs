using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MysBotSDK.MessageHandle.Info
{
	/// <summary>
	/// 实体消息
	/// </summary>
	public class Entity_Detail
	{
		/// <summary>
		/// 内嵌实体类型
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public EntityType type { get; set; }
		/// <summary>
		/// 实体类型
		/// </summary>
		public enum EntityType
		{
			/// <summary>
			/// 提及机器人
			/// </summary>
			mentioned_robot = 0,
			/// <summary>
			/// 提及用户
			/// </summary>
			mentioned_user = 1,
			/// <summary>
			/// 提及所有人
			/// </summary>
			mentioned_all = 2,
			/// <summary>
			/// 跳转房间
			/// </summary>
			villa_room_link = 3,
			/// <summary>
			/// 链接
			/// </summary>
			link = 4,
			/// <summary>
			/// 文字样式
			/// </summary>
			style = 5,
		}

		/// <summary>
		/// 机器人ID
		/// </summary>
		public string? bot_id { get; set; }

		/// <summary>
		/// 成员ID
		/// </summary>
		public string? user_id { get; set; }

		/// <summary>
		/// 大别野ID
		/// </summary>
		public string? villa_id { get; set; }

		/// <summary>
		/// 房间ID
		/// </summary>
		public string? room_id { get; set; }

		/// <summary>
		/// 跳转外部链接
		/// </summary>
		public string? url { get; set; }

		/// <summary>
		/// 跳转url时用户设置的高亮文本
		/// </summary>
		public string? url_highlight_text { get; set; }

		/// <summary>
		/// 字段为true时，跳转链接会带上含有用户信息的token
		/// </summary>
		public bool requires_bot_access_token { get; set; }

		/// <summary>
		/// 文字样式
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public Font_Sytle font_style { get; set; }
		/// <summary>
		/// 文字样式类型
		/// </summary>
		[Flags]
		public enum Font_Sytle
		{
			/// <summary>
			/// 无
			/// </summary>
			None=0,
			/// <summary>
			/// 加粗
			/// </summary>
			bold = 1,
			/// <summary>
			///斜体
			/// </summary>
			italic = 2,
			/// <summary>
			/// 删除线
			/// </summary>
			strikethrough = 4,
			/// <summary>
			/// 下滑线
			/// </summary>
			underline = 8,
		}
	}
}
