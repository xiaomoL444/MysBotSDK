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
		public string quoted_message_id { get; set; }
		public Int64 quoted_message_send_time { get; set; }
		public string original_message_id { get; set; }
		public Int64 original_message_send_time { get; set; }
	}
}
