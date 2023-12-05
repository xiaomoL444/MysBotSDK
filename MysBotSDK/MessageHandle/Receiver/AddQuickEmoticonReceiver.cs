using MysBotSDK.MessageHandle.ExtendData;
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
	/// 消息被贴上表情事件接收器
	/// </summary>
	public class AddQuickEmoticonReceiver : MessageReceiverBase
	{
		/// <summary>
		/// 大别野ID
		/// </summary>
		public UInt64 Villa_ID => addQuickEmoticon.villa_id;
		/// <summary>
		/// 房间ID
		/// </summary>
		public UInt64 Room_ID => addQuickEmoticon.room_id;
		/// <summary>
		/// 用户UID
		/// </summary>
		public UInt64 UID => addQuickEmoticon.uid;
		/// <summary>
		/// 表情ID
		/// </summary>
		public int Emoticon_ID => addQuickEmoticon.emoticon_id;
		/// <summary>
		/// 表情名字
		/// </summary>
		public string Emoticon_Name => addQuickEmoticon.emoticon;
		/// <summary>
		/// 被回复的消息ID
		/// </summary>
		public string Msg_UID => addQuickEmoticon.msg_uid;
		/// <summary>
		/// 如果被回复的消息从属于机器人，则该字段不为空字符串
		/// </summary>
		public string Bot_ID => addQuickEmoticon.bot_msg_id;
		/// <summary>
		/// 是否是取消表情
		/// </summary>
		public bool Is_Cancel => addQuickEmoticon.is_cancel;
		internal AddQuickEmoticon addQuickEmoticon { get; set; }
		public AddQuickEmoticonReceiver(string message) : base(message)
		{
			addQuickEmoticon = GetExtendDataMsg<AddQuickEmoticon>(message);
			villa_id = addQuickEmoticon.villa_id;
			room_id = addQuickEmoticon.room_id;

			Logger.Log($"Receive [AddQuickEmotion] {Emoticon_Name} Form villa:{villa_id},room:{room_id}");
		}
	}

}
