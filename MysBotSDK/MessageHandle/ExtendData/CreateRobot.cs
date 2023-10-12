using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.ExtendData
{
	/// <summary>
	/// Bot加入大别野
	/// </summary>
	public class CreateRobot : IExtendData
	{
		/// <summary>
		/// 大别野 id
		/// </summary>
		public UInt64 villa_id { get; set; }
	}
}
