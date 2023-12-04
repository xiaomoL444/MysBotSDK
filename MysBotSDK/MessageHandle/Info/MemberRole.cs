using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class MemberRole
	{
		/// <summary>
		/// 身份组id
		/// </summary>
		[JsonProperty("id")]
		private object id_ { get; set; } = new object();
		[JsonIgnore]
		public UInt64 id { get { return UInt64.Parse(id_.ToString()!); } set { id_ = (object)value; } }

		/// <summary>
		/// 身份组名称
		/// </summary>
		public string? name { get; set; }

		/// <summary>
		/// 大别野id
		/// </summary>
		[JsonProperty("villa_id")]
		private object villa_id_ { get; set; } = new object();
		[JsonIgnore]
		public UInt64 villa_id { get { return UInt64.Parse(villa_id_.ToString()); } set { villa_id_ = (object)value; } }

		/// <summary>
		/// 身份组颜色
		/// </summary>
		public string? color { get; set; }

		/// <summary>
		/// 身份组类型
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public Role_type role_type { get; set; }

		/// <summary>
		/// 是否选择全部房间
		/// </summary>
		public bool is_all_room { get; set; }

		/// <summary>
		/// 指定的房间列表
		/// </summary>
		public List<UInt64> room_ids { get; set; }

		/// <summary>
		/// 该身份组下的成员数量
		/// </summary>
		[JsonProperty("member_num")]
		private int member_num_ { get; set; }
		[JsonIgnore]
		public string member_num { get { return member_num_.ToString(); } set { member_num_ = int.Parse(value); } }
		public List<Permissions> permissions { get; set; } = new List<Permissions>();
	}
}
