using MysBotSDK.MessageHandle.ExtendData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{
	/// <summary>
	/// 审核事件接收器
	/// </summary>
	public class AuditCallbackReceiver : MessageReceiverBase
	{
		/// <summary>
		/// 审核事件的ID
		/// </summary>
		public string Audit_ID => auditCallback.audit_id;
		/// <summary>
		/// Bot的ID
		/// </summary>
		public string Bot_ID => auditCallback.bot_tpl_id;
		/// <summary>
		/// 大别野ID
		/// </summary>
		public UInt64 Villa_ID => auditCallback.villa_id;
		/// <summary>
		/// 房价ID
		/// </summary>
		public UInt64 Room_ID => auditCallback.room_id;
		/// <summary>
		/// 用户UID（和审核接口调用方传入的值一致）
		/// </summary>
		public UInt64 User_ID => auditCallback.user_id;
		/// <summary>
		/// 透传数据（和审核接口调用方传入的值一致）
		/// </summary>
		public string Pass_Through => auditCallback.pass_through;
		/// <summary>
		/// 审核结果，0作兼容，1审核通过，2审核驳回
		/// </summary>
		public string Audit_Result => auditCallback.audit_id;
		internal AuditCallback auditCallback { get; set; }
		public AuditCallbackReceiver(string message) : base(message)
		{
		}
		internal override void Initialize(string message)
		{
			auditCallback = JsonConvert.DeserializeObject<AuditCallback>(message);
		}
	}

}
