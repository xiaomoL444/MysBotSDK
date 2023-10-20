using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	/// <summary>
	/// 图片
	/// </summary>
	public class PicContentInfo
	{
		/// <summary>
		/// 【必填】图片链接
		/// </summary>
		public string? url { get; set; }

		/// <summary>
		/// 【选填】图片大小，单位：像素。如果图片长宽比不超过 9:16 或 16:9 , 那么按照开发者定义比例展示缩略图
		/// </summary>
		public Size? size { get; set; }

		/// <summary>
		/// 【选填】原始图片的文件大小，单位：字节
		/// </summary>
		public class Size
		{
			public Size(int width, int height)
			{
				this.width = width;
				this.height = height;
			}
			public int width { get; set; }
			public int height { get; set; }
		}
		public int file_size { get; set; }
	}
}
