using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class BotMemberAccessInfo
	{
		/// <summary>
		/// 用户UID
		/// </summary>
		public UInt64 uid { get; set; }

		/// <summary>
		/// 大别野 ID
		/// </summary>
		public UInt64 villa_id { get; set; }

		/// <summary>
		/// 用户机器人访问凭证
		/// </summary>
		public string member_access_token { get; set; }

		/// <summary>
		/// 机器人模板 ID
		/// </summary>
		public string bot_tpl_id { get; set; }
	}
}
