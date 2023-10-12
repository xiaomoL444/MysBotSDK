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
		public string url { get; set; }
		public Size size { get; set; }
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
