using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class Panel
	{
		/// <summary>
		/// 模板id，通过创建消息组件模板接口，可以提前将组件面板保存，使用 template_id来快捷发送消息
		/// </summary>
		public UInt64 template_id { get; set; }

		/// <summary>
		/// 定义小型组件，即一行摆置3个组件，每个组件最多展示2个中文字符或4个英文字符
		/// </summary>
		public List<List<Component_Group>> small_component_group_list { get; set; } = new List<List<Component_Group>>();

		/// <summary>
		/// 定义中型组件，即一行摆置2个组件，每个组件最多展示4个中文字符或8个英文字符
		/// </summary>
		public List<List<Component_Group>> mid_component_group_list { get; set; } = new List<List<Component_Group>>();

		/// <summary>
		/// 定义大型组件，即一行摆置1个组件，每个组件最多展示10个中文字符或20个英文字符
		/// </summary>
		public List<List<Component_Group>> big_component_group_list { get; set; } = new List<List<Component_Group>>();

	}
}
