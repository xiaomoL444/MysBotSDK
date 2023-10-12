using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.ExtendData
{
	/// <summary>
	/// 审核时间
	/// </summary>
	public class AuditCallback : IExtendData
	{
		/// <summary>
		/// 审核事件 id
		/// </summary>
		public string audit_id { get; set; }
		/// <summary>
		/// 机器人 id
		/// </summary>
		public string bot_tpl_id { get; set; }
		/// <summary>
		/// 大别野 id
		/// </summary>
		public UInt64 villa_id { get; set; }
		/// <summary>
		/// 房间 id（和审核接口调用方传入的值一致）
		/// </summary>
		public UInt64 room_id { get; set; }
		/// <summary>
		/// 用户 id（和审核接口调用方传入的值一致）
		/// </summary>
		public UInt64 user_id { get; set; }
		/// <summary>
		/// 透传数据（和审核接口调用方传入的值一致）
		/// </summary>
		public string pass_through { get; set; }
		/// <summary>
		/// 审核结果，0作兼容，1审核通过，2审核驳回
		/// </summary>
		public int audit_result { get; set; }
	}
}
