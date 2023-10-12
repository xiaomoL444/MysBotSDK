using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class MemberBasic
	{
		/// <summary>
		/// 用户uid
		/// </summary>
		public UInt64 uid { get; set; }
		/// <summary>
		/// 昵称
		/// </summary>
		public string nickname { get; set; }
		/// <summary>
		/// 个性签名
		/// </summary>
		public string introduce { get; set; }
		/// <summary>
		/// 头像链接
		/// </summary>
		public string avatar_url { get; set; }
	}
}
