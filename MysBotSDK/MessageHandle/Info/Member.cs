using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class Member
	{
		/// <summary>
		/// 用户基本信息
		/// </summary>
		public MemberBasic? basic { get; set; }

		/// <summary>
		/// 用户加入的身份组id列表
		/// </summary>
		public List<UInt64> role_id_list { get; set; } = new List<UInt64>();

		/// <summary>
		/// 用户加入时间 ISO8601 timestamp
		/// </summary>
		public string? joined_at { get; set; }

		/// <summary>
		/// 用户已加入的身份组列表
		/// </summary>
		public List<MemberRole> role_list { get; set; } = new List<MemberRole>();
	}
}
