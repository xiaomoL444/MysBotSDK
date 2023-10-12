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
		public UInt64 room_id { get; set; }
		public string room_name { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public Room_Type room_type { get; set; }
		public enum Room_Type
		{
			BOT_PLATFORM_ROOM_TYPE_CHAT_ROOM = 0,
			BOT_PLATFORM_ROOM_TYPE_POST_ROOM = 1,
			BOT_PLATFORM_ROOM_TYPE_SCENE_ROOM = 2,
			BOT_PLATFORM_ROOM_TYPE_INVALID = 3
		}
		public UInt64 group_id { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public Room_Default_Notify_Type room_default_notify_type { get; set; }
		public enum Room_Default_Notify_Type
		{
			BOT_PLATFORM_DEFAULT_NOTIFY_TYPE_NOTIFY = 0,
			BOT_PLATFORM_DEFAULT_NOTIFY_TYPE_IGNORE = 1,
			BOT_PLATFORM_DEFAULT_NOTIFY_TYPE_INVALID = 2
		}
		public SendMsgAuthRange send_msg_auth_range { get; set; }
		public class SendMsgAuthRange
		{
			public bool is_all_send_msg { get; set; }
			public List<UInt64> roles { get; set; }
		}
	}
}
