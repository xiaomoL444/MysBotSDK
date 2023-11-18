using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	/// <summary>
	/// 被提及的类型
	/// </summary>
	public enum MentionType
	{
		/// <summary>
		/// 无提及
		/// </summary>
		None = 0,

		/// <summary>
		/// 提及全体成员
		/// </summary>
		All = 1,

		/// <summary>
		/// 提及部分成员
		/// </summary>
		Partof = 2,
	}
}
