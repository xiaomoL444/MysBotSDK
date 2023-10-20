using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class Room
	{
		/// <summary>
		/// 房间ID
		/// </summary>
		public UInt64 room_id { get; set; }

		/// <summary>
		/// 房间名称
		/// </summary>
		public string? room_name { get; set; }

		/// <summary>
		/// 房间类型
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public Room_Type room_type { get; set; }
		public enum Room_Type
		{
			BOT_PLATFORM_ROOM_TYPE_CHAT_ROOM = 0,
			BOT_PLATFORM_ROOM_TYPE_POST_ROOM = 1,
			BOT_PLATFORM_ROOM_TYPE_SCENE_ROOM = 2,
			BOT_PLATFORM_ROOM_TYPE_INVALID = 3
		}

		/// <summary>
		/// 分组ID
		/// </summary>
		public UInt64 group_id { get; set; }

		/// <summary>
		/// 房间默认通知类型
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public Room_Default_Notify_Type room_default_notify_type { get; set; }
		public enum Room_Default_Notify_Type
		{
			BOT_PLATFORM_DEFAULT_NOTIFY_TYPE_NOTIFY = 0,
			BOT_PLATFORM_DEFAULT_NOTIFY_TYPE_IGNORE = 1,
			BOT_PLATFORM_DEFAULT_NOTIFY_TYPE_INVALID = 2
		}

		/// <summary>
		/// 房间消息发送权限范围设置
		/// </summary>
		public SendMsgAuthRange? send_msg_auth_range { get; set; }
		public class SendMsgAuthRange
		{
			/// <summary>
			/// 是否全局可发送
			/// </summary>
			public bool is_all_send_msg { get; set; }

			/// <summary>
			/// 可发消息的身份组ID
			/// </summary>
			public List<UInt64> roles { get; set; }
		}
	}
}
