using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	/// <summary>
	/// 引用消息
	/// </summary>
	public class QuoteInfo
	{
		/// <summary>
		/// 引用消息ID
		/// </summary>
		public string? quoted_message_id { get; set; }

		/// <summary>
		/// 引用消息发送时间戳
		/// </summary>
		public Int64 quoted_message_send_time { get; set; }

		/// <summary>
		/// 引用树初始消息 id，和 quoted_message_id 保持一致即可
		/// </summary>
		public string? original_message_id { get; set; }

		/// <summary>
		/// 引用树初始消息发送时间戳，和 quoted_message_send_time 保持一致即可
		/// </summary>
		public Int64 original_message_send_time { get; set; }
	}
}
