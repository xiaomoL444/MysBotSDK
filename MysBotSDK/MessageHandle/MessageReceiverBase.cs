using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.MessageHandle.Info;
using System.Runtime.CompilerServices;

namespace MysBotSDK.MessageHandle;

public class MessageReceiverBase
{
	/// <summary>
	/// robot信息
	/// </summary>
	public Robot? robot { get; set; }

	/// <summary>
	/// 事件类型
	/// </summary>
	public EventType EventType { get; private set; }

	/// <summary>
	/// 接收器
	/// </summary>
	internal MessageReceiverBase? receiver { get; set; }

	/// <summary>
	/// 大别野ID
	/// </summary>
	internal UInt64 villa_id { get; set; }

	/// <summary>
	/// 房间ID
	/// </summary>
	internal UInt64 room_id { get; set; }

	/// <summary>
	/// 构造器
	/// </summary>
	/// <param name="message"></param>
	public MessageReceiverBase(string message)
	{
		Initialize(message);
	}

	/// <summary>
	/// 初始化(主要是为了多态接收器重构准备的)
	/// </summary>
	/// <param name="message"></param>
	internal virtual void Initialize(string message)
	{
		var json = JObject.Parse(message)["event"];
		robot = JsonConvert.DeserializeObject<Robot>(json!["robot"]!.ToString())!;
		EventType = (EventType)(int)json["type"]!;

		var eventData = json["extend_data"]!["EventData"];

		//对事件数据赋值
		switch (EventType)
		{
			case EventType.JoinVilla:
				receiver = new JoinVillaReceiver(eventData!["JoinVilla"]!.ToString());
				var joinVillaReceiver = (JoinVillaReceiver)receiver;
				villa_id = joinVillaReceiver.villa_id;
				room_id = 0;
				break;
			case EventType.SendMessage:
				receiver = new SendMessageReceiver(eventData!["SendMessage"]!.ToString());
				var sendMessageReceiver = (SendMessageReceiver)receiver;
				villa_id = sendMessageReceiver.villa_id;
				room_id = sendMessageReceiver.Room_ID;
				break;
			case EventType.CreateRobot:
				receiver = new CreateRobotReceiver(eventData!["CreateRobot"]!.ToString());
				var createRobotReceiver = (CreateRobotReceiver)receiver;
				villa_id = createRobotReceiver.villa_id;
				room_id = 0;
				break;
			case EventType.DeleteRobot:
				receiver = new DeleteRobotReceiver(eventData!["DeleteRobot"]!.ToString());
				var deleteRobotReceiver = (DeleteRobotReceiver)receiver;
				villa_id = deleteRobotReceiver.villa_id;
				room_id = 0;
				break;
			case EventType.AddQuickEmoticon:
				receiver = new AddQuickEmoticonReceiver(eventData!["AddQuickEmoticon"]!.ToString());
				var addQuickEmoticonReceiver = (AddQuickEmoticonReceiver)receiver;
				villa_id = addQuickEmoticonReceiver.villa_id;
				room_id = addQuickEmoticonReceiver.Room_ID;
				break;
			case EventType.AuditCallback:
				receiver = new AuditCallbackReceiver(eventData!["AuditCallback"]!.ToString());
				var auditCallbackReceiver = (AuditCallbackReceiver)receiver;
				villa_id = auditCallbackReceiver.villa_id;
				room_id = auditCallbackReceiver.Room_ID;
				break;
			default:
				break;
		}
	}
}

/// <summary>
/// 事件类型
/// </summary>
public enum EventType
{
	JoinVilla = 1,
	SendMessage = 2,
	CreateRobot = 3,
	DeleteRobot = 4,
	AddQuickEmoticon = 5,
	AuditCallback = 6,
}