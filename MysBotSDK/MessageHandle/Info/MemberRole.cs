using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	/// <summary>
	/// 身份组信息( 获取大别野下所有身份组 接口使用 )
	/// </summary>
	public class MemberRole
	{
		[JsonProperty("id")]
		private object id_ { get; set; } = new object();

		/// <summary>
		/// 身份组id
		/// </summary>
		[JsonIgnore]
		public UInt64 id { get { return UInt64.Parse(id_.ToString()!); } set { id_ = (object)value; } }

		/// <summary>
		/// 身份组名称
		/// </summary>
		public string? name { get; set; }

		[JsonProperty("villa_id")]
		private object villa_id_ { get; set; } = new object();

		/// <summary>
		/// 大别野id
		/// </summary>
		[JsonIgnore]
		public UInt64 villa_id { get { return UInt64.Parse(villa_id_.ToString()!); } set { villa_id_ = (object)value; } }

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
		public List<UInt64> room_ids { get; set; } = new();
	}
}
