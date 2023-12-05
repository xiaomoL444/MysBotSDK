using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	/// <summary>
	/// 身份组信息( 获取身份组 接口使用 )
	/// </summary>
	public class MemberRoleInfo : MemberRole
	{
		[JsonProperty("member_num")]
		private int member_num_ { get; set; }

		/// <summary>
		/// 该身份组下的成员数量
		/// </summary>
		[JsonIgnore]
		public string member_num { get { return member_num_.ToString(); } set { member_num_ = int.Parse(value); } }

		/// <summary>
		/// 身份组拥有的权限列表
		/// </summary>
		public List<Permissions> permissions { get; set; } = new List<Permissions>();
	}
}
