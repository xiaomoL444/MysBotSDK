using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class Villa
	{
		/// <summary>
		/// 大别野id
		/// </summary>
		public UInt64 villa_id { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		public string? name { get; set; }

		/// <summary>
		/// 别野头像链接
		/// </summary>
		public string? villa_avatar_url { get; set; }

		/// <summary>
		/// 别野主人id
		/// </summary>
		public UInt64 owner_uid { get; set; }

		/// <summary>
		/// 是否是官方别野
		/// </summary>
		public bool is_official { get; set; }

		/// <summary>
		/// 介绍
		/// </summary>
		public string? introduce { get; set; }

		public UInt32 category_id { get; set; }

		/// <summary>
		/// 标签
		/// </summary>
		public List<string> tags { get; set; } = new List<string>();
	}
}
