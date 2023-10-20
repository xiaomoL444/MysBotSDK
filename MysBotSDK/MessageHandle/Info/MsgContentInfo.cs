using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class MsgContentInfo
	{
		/// <summary>
		/// 消息内容
		/// </summary>
		public MsgContent? content { get; set; }

		/// <summary>
		/// 消息的提及信息
		/// </summary>
		public MentionedInfo? mentionedInfo { get; set; }

		/// <summary>
		/// 引用消息的信息
		/// </summary>
		public QuoteInfo? quote { get; set; }
	}
}
