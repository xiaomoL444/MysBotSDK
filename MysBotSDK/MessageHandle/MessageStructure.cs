using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MysBotSDK.MessageHandle
{
	#region 接收消息
	/// <summary>
	/// 新用户加入大别野消息
	/// </summary>

	public class JoinVilla : IExtendData
	{
		/// <summary>
		///用户 id
		/// </summary>
		public uint join_uid { get; set; }
		/// <summary>
		///用户昵称
		/// </summary>
		public string? join_user_nickname { get; set; }
		/// <summary>
		///用户加入时间的时间戳
		/// </summary>
		public long join_at { get; set; }
		/// <summary>
		/// 大别野 id
		/// </summary>
		public string? villa_id { get; set; }
	}
	public class SendMessage : IExtendData
	{
		public List<string> args { get; set; }
		/// <summary>
		/// 消息内容string
		/// </summary>
		[JsonProperty("content")]
		private string? content_ { get; set; }
		/// <summary>
		/// 消息内容(反序列化)
		/// </summary>
		[JsonIgnore]
		public Content content { get { return JsonConvert.DeserializeObject<Content>(content_); } }
		public class Content
		{
			public MsgContent content { get; set; }
			public MentionedInfo mentionedInfo { get; set; }
		}
		/// <summary>
		/// 发送者 id
		/// </summary>
		public uint from_user_id { get; set; }
		/// <summary>
		/// 发送时间的时间戳
		/// </summary>
		public long send_at { get; set; }
		/// <summary>
		/// 房间 id
		/// </summary>
		public ulong room_id { get; set; }
		/// <summary>
		/// 目前只支持文本类型消息
		/// </summary>
		public string? object_name { get; set; }
		/// <summary>
		/// 用户昵称
		/// </summary>
		public string? nickname { get; set; }
		/// <summary>
		/// 消息 id
		/// </summary>
		public string? msg_uid { get; set; }
		/// <summary>
		/// 如果被回复的消息从属于机器人，则该字段不为空字符串
		/// </summary>
		public string? bot_msg_id { get; set; }
		/// <summary>
		/// 大别野 id
		/// </summary>
		public int villa_id { get; set; }
		/// <summary>
		/// 引用信息
		/// </summary>
		public Quote_msg? quote_msg { get; set; }

		/// <summary>
		/// 回调消息引用消息的基础信息
		/// </summary>
		public class Quote_msg
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
			public string? msg_type { get; set; }
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
	public class CreateRobot : IExtendData
	{
		/// <summary>
		/// 大别野 id
		/// </summary>
		public int villa_id { get; set; }
	}
	public class DeleteRobot : IExtendData
	{
		/// <summary>
		/// 大别野 id
		/// </summary>
		public int villa_id { get; set; }
	}
	public class AddQuickEmoticon : IExtendData
	{
		/// <summary>
		/// 大别野 id
		/// </summary>
		public int villa_id { get; set; }
		/// <summary>
		/// 房间 id
		/// </summary>
		public int room_id { get; set; }
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
		public string emoticon { get; set; }
		/// <summary>
		/// 被回复的消息 id
		/// </summary>
		public string msg_uid { get; set; }
		/// <summary>
		/// 如果被回复的消息从属于机器人，则该字段不为空字符串
		/// </summary>
		public string bot_msg_id { get; set; }
		/// <summary>
		/// 是否是取消表情
		/// </summary>
		public string is_cancel { get; set; }
	}
	public class AuditCallback : IExtendData
	{
		/// <summary>
		/// 审核事件 id
		/// </summary>
		public string audit_id { get; set; }
		/// <summary>
		/// 机器人 id
		/// </summary>
		public string bot_tpl_id { get; set; }
		/// <summary>
		/// 大别野 id
		/// </summary>
		public int villa_id { get; set; }
		/// <summary>
		/// 房间 id（和审核接口调用方传入的值一致）
		/// </summary>
		public int room_id { get; set; }
		/// <summary>
		/// 用户 id（和审核接口调用方传入的值一致）
		/// </summary>
		public int user_id { get; set; }
		/// <summary>
		/// 透传数据（和审核接口调用方传入的值一致）
		/// </summary>
		public string pass_through { get; set; }
		/// <summary>
		/// 审核结果，0作兼容，1审核通过，2审核驳回
		/// </summary>
		public int audit_result { get; set; }
	}

	#endregion

	#region 发送消息

	/// <summary>
	/// 提醒消息(@At)
	/// </summary>
	public class MentionedInfo
	{
		//[JsonConverter(typeof(int))]
		public MentionType type { get; set; }
		public enum MentionType
		{
			All = 1,
			Partof = 2,
		}
		public List<string> userIdList { get; set; } = new List<string>();
	}
	/// <summary>
	/// 引用消息
	/// 引用消息
	/// </summary>
	public class QuoteInfo
	{
		public string quoted_message_id { get; set; }
		public int quoted_message_send_time { get; set; }
		public string original_message_id { get; set; }
		public int original_message_send_time { get; set; }
	}
	public class MsgContent
	{
		public string text { get; set; }
		public List<Entity> entities { get; set; }
	}
	public class Entity
	{
		public entity_detail entity { get; set; }
		public class entity_detail
		{
			[JsonConverter(typeof(StringEnumConverter))]
			public EntityType type { get; set; }
			public enum EntityType
			{
				mentioned_robot = 0,
				mentioned_user = 1,
				mentioned_all = 2,
				villa_room_link = 3,
				link = 4
			}
			public string bot_id { get; set; }
			public string user_id { get; set; }
			public string villa_id { get; set; }
			public string room_id { get; set; }
			public string url { get; set; }
			public bool requires_bot_access_token { get; set; }
		}
		public ulong length { get; set; }
		public ulong offset { get; set; }
	}
	#endregion
	#region 回应
	/// <summary>
	/// ????
	/// </summary>
	public class Bot_msg_id
	{
		/// <summary>
		/// 
		/// </summary>
		public string bot_msg_id { get; set; }
	}

	public class Response
	{
		/// <summary>
		/// 
		/// </summary>
		public int retcode { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string message { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string data { get; set; }
	}
	#endregion
	#region 共用

	/// <summary>
	/// 消息体
	/// </summary>
	public class MsgContentInfo
	{
		public MsgContent content { get; set; }
		public MentionedInfo mentionedInfo { get; set; }
		public QuoteInfo quote { get; set; }
	}
	/// <summary>
	/// 图片
	/// </summary>
	public class PicContentInfo
	{
		public string url { get; set; }
		public Size size { get; set; }
		public class Size
		{
			public Size(int width, int height)
			{
				this.width = width;
				this.height = height;
			}
			public int width { get; set; }
			public int height { get; set; }
		}
		public int file_size { get; set; }
	}
	#region 用户
	public class MemberBasic
	{
		/// <summary>
		/// 用户uid
		/// </summary>
		public UInt64 uid { get; set; }
		/// <summary>
		/// 昵称
		/// </summary>
		public string nickname { get; set; }
		/// <summary>
		/// 个性签名
		/// </summary>
		public string introduce { get; set; }
		/// <summary>
		/// 头像链接
		/// </summary>
		public string avatar_url { get; set; }
	}
	public class Member
	{
		/// <summary>
		/// 用户基本信息
		/// </summary>
		public MemberBasic basic { get; set; }
		/// <summary>
		/// 用户加入的身份组id列表
		/// </summary>
		public List<UInt64> role_id_list { get; set; }
		/// <summary>
		/// 用户加入时间 ISO8601 timestamp
		/// </summary>
		public string joined_at { get; set; }
		/// <summary>
		/// 用户已加入的身份组列表
		/// </summary>
		public List<MemberRole> role_list { get; set; }
	}
	public class MemberRole
	{
		/// <summary>
		/// 身份组id
		/// </summary>
		public UInt64 id { get; set; }
		/// <summary>
		/// 身份组名称
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// 大别野id
		/// </summary>
		public UInt64 villa_id { get; set; }
		/// <summary>
		/// 身份组颜色
		/// </summary>
		public string color { get; set; }
		/// <summary>
		/// 身份组类型
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public Role_type role_type { get; set; }
		public enum Role_type
		{
			MEMBER_ROLE_TYPE_ALL_MEMBER = 0,//所有人身份组
			MEMBER_ROLE_TYPE_ADMIN = 1,//管理员身份组
			MEMBER_ROLE_TYPE_OWNER = 2,//大别野房主身份组
			MEMBER_ROLE_TYPE_CUSTOM = 3,//其他自定义身份组
			MEMBER_ROLE_TYPE_UNKNOWN = 4//未知
		}
		/// <summary>
		/// 是否选择全部房间
		/// </summary>
		public bool is_all_room { get; set; }
		/// <summary>
		/// 指定的房间列表
		/// </summary>
		public UInt64 rool_ids { get; set; }
		#endregion
	}
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
	#endregion
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
}