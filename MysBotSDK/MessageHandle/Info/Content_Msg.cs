using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	/// <summary>
	/// 消息体
	/// </summary>
	public class Content_Msg
	{
		public MsgContent content { get; set; }
		public MentionedInfo mentionedInfo { get; set; }
		public QuoteInfo QuoteInfo { get; set; }
	}
}
