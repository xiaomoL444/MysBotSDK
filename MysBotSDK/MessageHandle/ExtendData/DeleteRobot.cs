using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.ExtendData
{
	/// <summary>
	/// 大别野删除机器人
	/// </summary>
	public class DeleteRobot : IExtendData
	{
		/// <summary>
		/// 大别野 id
		/// </summary>
		public UInt64 villa_id { get; set; }
	}
}
