using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MysBotSDK.MessageHandle.Info
{

	public abstract class Component_Group
	{
		[JsonProperty()]
		internal int c_type { get; set; }
		/// <summary>
		/// 组件id，由机器人自定义，不能为空字符串。面板内的id需要唯一
		/// </summary>
		public string? id { get; set; }

		/// <summary>
		/// 组件展示文本, 不能为空
		/// </summary>
		public string? text { get; set; }

		/// <summary>
		/// 组件类型，目前支持 type=1 按钮组件，未来会扩展更多组件类型
		/// </summary>
		public Button_Type type { get; set; }

		/// <summary>
		/// 是否订阅该组件的回调事件
		/// </summary>
		public bool need_callback { get; set; }

		/// <summary>
		/// 组件回调透传信息，由机器人自定义
		/// </summary>
		public string? extra { get; set; }
	}

	/// <summary>
	/// 回调型按钮
	/// </summary>
	public class CallBack_Button : Component_Group
	{
		public CallBack_Button()
		{
			c_type = 1;
		}
	}
	/// <summary>
	/// 输入型按钮
	/// </summary>
	public class Input_Button : Component_Group
	{

		public Input_Button()
		{
			c_type = 2;
		}
		/// <summary>
		/// 如果交互类型为输入型，则需要在该字段填充输入内容，不能为空
		/// </summary>
		public string? input { get; set; }
	}

	/// <summary>
	/// 跳转型按钮
	/// </summary>
	public class Link_Button : Component_Group
	{
		public Link_Button()
		{
			c_type = 3;
		}
		/// <summary>
		/// 如果交互类型为跳转型，需要在该字段填充跳转链接，不能为空
		/// </summary>
		public string? link { get; set; }

		/// <summary>
		/// 对于跳转链接来说，如果希望携带用户信息token，则need_token设置为true
		/// </summary>
		public bool need_token { get; set; }
	}
	public enum Button_Type
	{
		Normal_Type = 1,
	}
}