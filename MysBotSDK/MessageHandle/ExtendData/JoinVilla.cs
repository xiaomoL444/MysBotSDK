using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.ExtendData
{
	/// <summary>
	/// 新用户加入大别野消息
	/// </summary>

	public class JoinVilla : IExtendData
	{
		/// <summary>
		///用户 id
		/// </summary>
		public uint join_uid { get; set; }
		/// <summary>
		///用户昵称
		/// </summary>
		public string? join_user_nickname { get; set; }
		/// <summary>
		///用户加入时间的时间戳
		/// </summary>
		public long join_at { get; set; }
		/// <summary>
		/// 大别野 id
		/// </summary>
		public UInt64 villa_id { get; set; }
	}
}
