using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	/// <summary>
	/// 提醒消息(@At)
	/// </summary>
	public class MentionedInfo
	{
		public MentionType type { get; set; }
		public List<string> userIdList { get; set; } = new List<string>();
	}
}
