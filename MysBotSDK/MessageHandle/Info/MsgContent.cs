using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class MsgContent
	{
		public string text { get; set; }
		public List<Entity> entities { get; set; }
	}
}
