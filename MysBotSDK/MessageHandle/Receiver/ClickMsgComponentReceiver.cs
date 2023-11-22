using MysBotSDK.MessageHandle.ExtendData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{
	public class ClickMsgComponentReceiver : MessageReceiverBase
	{
		/// <summary>
		/// 用户UID
		/// </summary>
		public UInt64 UID => clickMsgComponent.uid;

		/// <summary>
		/// 房间ID
		/// </summary>
		public UInt64 Room_id => clickMsgComponent.room_id;
		/// <summary>
		/// 大别野ID
		/// </summary>
		public UInt64 Villa_ID => clickMsgComponent.villa_id;
		/// <summary>
		/// 消息ID
		/// </summary>
		public string Msg_uid => clickMsgComponent.msg_uid;
		/// <summary>
		/// 如果消息从属于机器人，则该字段不为空字符串
		/// </summary>
		public string Bot_Msg_ID => clickMsgComponent.bot_msg_id;
		/// <summary>
		/// 机器人自定义的组件id
		/// </summary>
		public string Component_Id => clickMsgComponent.component_id;
		/// <summary>
		/// 如果该组件模板为已创建模板，则template_id不为0
		/// </summary>
		[JsonConverter(typeof(String))]
		public UInt64 Template_Id => clickMsgComponent.template_id;
		/// <summary>
		/// 机器人自定义透传信息
		/// </summary>
		public string Extra => clickMsgComponent.extra;
		internal ClickMsgComponent clickMsgComponent { get; set; }
		public ClickMsgComponentReceiver(string message) : base(message)
		{
			clickMsgComponent = GetExtendDataMsg<ClickMsgComponent>(message);
			villa_id = clickMsgComponent.villa_id;
			room_id = clickMsgComponent.room_id;
		}
	}
}
