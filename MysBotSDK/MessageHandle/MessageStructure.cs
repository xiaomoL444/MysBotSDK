using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MysBotSDK.MessageHandle
{
	#region 大别野
	public class Villa
	{
		/// <summary>
		/// 大别野id
		/// </summary>
		public UInt64 villa_id { get; set; }
		/// <summary>
		/// 名称
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// 别野头像链接
		/// </summary>
		public string villa_avatar_url { get; set; }
		/// <summary>
		/// 别野主人id
		/// </summary>
		public UInt64 owner_uid { get; set; }
		/// <summary>
		/// 是否是官方别野
		/// </summary>
		public bool is_official { get; set; }
		/// <summary>
		/// 介绍
		/// </summary>
		public string introduce { get; set; }
		public UInt32 category_id { get; set; }
		/// <summary>
		/// 标签
		/// </summary>
		public List<string> tags { get; set; }
	}
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
	public class Group
	{
		public string group_name { get; set; }
		public UInt64 group_id { get; set; }
	}
	#endregion

	public class BotMemberAccessInfo
	{
		public UInt64 uid { get; set; }
		public UInt64 villa_id { get; set; }
		public string member_access_token { get; set; }
		public string bot_tpl_id { get; set; }
	}

	public class Emoticon
	{
		public string describe_text { get; set; }
		public string icon { get; set; }
		public UInt64 emoticon_id { get; set; }
	}
	public enum Content_Type
	{
		AuditContentTypeText = 0,
		AuditContentTypeImage = 1
	}
	public class Robot
	{
		/// <summary>
		/// Bot信息
		/// </summary>
		public Template template { get; set; }
		/// <summary>
		/// 大别野ID
		/// </summary>
		public int villa_id { get; set; }
	}
	public class Template
	{
		/// <summary>
		/// BotID
		/// </summary>
		public string id { get; set; }
		/// <summary>
		/// Bot名字
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// Bot描述
		/// </summary>
		public string desc { get; set; }
		/// <summary>
		/// Bot头像
		/// </summary>
		public string icon { get; set; }
		/// <summary>
		/// 用户使用的快捷命令
		/// </summary>
		public List<CommandsItem> commands { get; set; }
	}
	public class CommandsItem
	{
		/// <summary>
		/// commond命令
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// commond描述
		/// </summary>
		public string desc { get; set; }
	}
}