using MysBotSDK.MessageHandle.ExtendData;
using MysBotSDK.MessageHandle.Info;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{
	/// <summary>
	/// 有用户发送@At消息事件接收器
	/// </summary>
	public class SendMessageReceiver : MessageReceiverBase
	{
		/// <summary>
		/// 用户提交的指令参数(不包括开头的@At与/指令，只包括指令后的参数，以空格分离)
		/// </summary>
		public List<string> args => sendMessage.args;
		/// <summary>
		/// 反序列化后的content消息，非必要不使用，里面似乎还有类没实现...
		/// </summary>
		public Content_Msg content => sendMessage.content;
		/// <summary>
		/// 消息文本
		/// </summary>
		public string Text => sendMessage.content.content.text;
		/// <summary>
		/// 用户的UID
		/// </summary>
		public UInt64 UID => sendMessage.from_user_id;
		/// <summary>
		/// 消息的发送时间
		/// </summary>
		public Int64 Send_Time => sendMessage.send_at;
		/// <summary>
		/// 房间ID
		/// </summary>
		public UInt64 Room_ID => sendMessage.room_id;
		/// <summary>
		/// 用户昵称
		/// </summary>
		public string Name => sendMessage.nickname;
		/// <summary>
		/// 消息ID
		/// </summary>
		public string Msg_ID => sendMessage.msg_uid;
		/// <summary>
		/// 如果被回复的消息从属于机器人，则该字段不为空字符串
		/// </summary>
		public string Bot_ID => sendMessage.bot_msg_id;
		/// <summary>
		/// 大别野ID
		/// </summary>
		public UInt64 Villa_ID => sendMessage.villa_id;
		/// <summary>
		/// 消息摘要，如果是文本消息，则返回消息的文本内容。如果是图片消息，则返回"[图片]"
		/// </summary>
		public string Quote_Content => sendMessage.quote_msg.content;
		/// <summary>
		/// 消息ID
		/// </summary>
		public string Quote_Msg_ID => sendMessage.quote_msg.msg_uid;
		/// <summary>
		/// 如果消息从属于机器人，则该字段不为空字符串
		/// </summary>
		public string Quote_Bot_ID => sendMessage.quote_msg.bot_msg_id;
		/// <summary>
		/// 发送时间的时间戳 
		/// </summary>
		public Int64 Quote_Send_Time => sendMessage.quote_msg.send_at;
		/// <summary>
		/// 消息类型，包括"文本"，"图片"，"帖子卡片"等
		/// </summary>
		public Quote_Msg.Msg_Type Quote_Msg_Type => sendMessage.quote_msg.msg_type;
		/// <summary>
		/// 发送者的UID
		/// </summary>
		public UInt64 Quote_UID => sendMessage.quote_msg.from_user_id;
		/// <summary>
		/// 发送者昵称
		/// </summary>
		public string Quote_Name => sendMessage.quote_msg.from_user_nickname;
		/// <summary>
		/// 发送者 id（字符串）可携带机器人发送者的id
		/// </summary>
		public string User_ID_Str => sendMessage.quote_msg.from_user_id_str;

		internal SendMessage sendMessage { get; set; }
		internal Quote_Msg quote_msg => sendMessage.quote_msg;

		public SendMessageReceiver(string message) : base(message)
		{
		}
		public override void Initialize(string message)
		{
			sendMessage = JsonConvert.DeserializeObject<SendMessage>(message)!;
			var args = sendMessage!.content.content.text.Split(" ").ToList();
			args.RemoveRange(0, 2);
			sendMessage.args = args;
		}
	}
}
