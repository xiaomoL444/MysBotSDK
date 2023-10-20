using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class MsgContent
	{
		/// <summary>
		/// 消息文本
		/// </summary>
		public string? text { get; set; }

		/// <summary>
		/// 消息文本内嵌的实体信息
		/// </summary>
		public List<Entity> entities { get; set; } = new List<Entity>();
	}
}
