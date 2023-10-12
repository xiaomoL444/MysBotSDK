using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class BotMemberAccessInfo
	{
		public UInt64 uid { get; set; }
		public UInt64 villa_id { get; set; }
		public string member_access_token { get; set; }
		public string bot_tpl_id { get; set; }
	}
}
