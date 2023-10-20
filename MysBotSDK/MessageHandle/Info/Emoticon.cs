using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class Emoticon
	{
		/// <summary>
		/// 描述文本
		/// </summary>
		public string describe_text { get; set; }

		/// <summary>
		/// 表情图片链接
		/// </summary>
		public string icon { get; set; }

		/// <summary>
		/// 表情 ID
		/// </summary>
		public UInt64 emoticon_id { get; set; }
	}
}
