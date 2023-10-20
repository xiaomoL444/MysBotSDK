using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class Template
	{
		/// <summary>
		/// BotID
		/// </summary>
		public strin	 id { get; set; }

		/// <summary>
		/// Bot名字
		/// </summary>
		public string? name { get; set; }

		/// <summary>
		/// Bot描述
		/// </summary>
		public string? desc { get; set; }

		/// <summary>
		/// Bot头像
		/// </summary>
		public string? icon { get; set; }

		/// <summary>
		/// 用户使用的快捷命令
		/// </summary>
		public List<CommandsItem> commands { get; set; } = new List<CommandsItem>();
	}

}
