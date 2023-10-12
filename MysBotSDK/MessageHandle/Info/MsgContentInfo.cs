using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class MsgContentInfo
	{
		public MsgContent content { get; set; }
		public MentionedInfo mentionedInfo { get; set; }
		public QuoteInfo quote { get; set; }
	}
}
