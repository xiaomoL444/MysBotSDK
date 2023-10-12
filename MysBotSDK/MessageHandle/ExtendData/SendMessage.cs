using MysBotSDK.MessageHandle.Info;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.ExtendData
{
	public class SendMessage : IExtendData
	{
		public List<string> args { get; set; } = new List<string>();
		/// <summary>
		/// 消息内容string
		/// </summary>
		[JsonProperty("content")]
		private string? content_ { get; set; }
		/// <summary>
		/// 消息内容(反序列化)
		/// </summary>
		[JsonIgnore]
		public Content_Msg content { get { return JsonConvert.DeserializeObject<Content_Msg>(content_); } }
		/// <summary>
		/// 发送者 id
		/// </summary>
		public UInt64 from_user_id { get; set; }
		/// <summary>
		/// 发送时间的时间戳
		/// </summary>
		public Int64 send_at { get; set; }
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
		public UInt64 villa_id { get; set; }
		/// <summary>
		/// 引用信息
		/// </summary>
		public Quote_Msg? quote_msg { get; set; }
	}
}
