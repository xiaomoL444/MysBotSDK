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
		/// <summary>
		/// 提及类型
		/// </summary>
		public MentionType type { get; set; }

		/// <summary>
		/// 如果不是提及全员，应该填写被提及的用户 id 列表
		/// </summary>
		public List<string> userIdList { get; set; } = new List<string>();
	}
}
