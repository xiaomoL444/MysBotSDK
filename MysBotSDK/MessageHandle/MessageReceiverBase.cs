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
using MysBotSDK.MessageHandle.ExtendData;
using Google.Protobuf;
using Newtonsoft.Json.Serialization;

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
	public MessageReceiverBase(vila_bot.RobotEventMessage robotEventMessage)
	{
		Initialize(robotEventMessage);
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
		var this_ = this;
		receiver = (JoinVillaReceiver)this_;
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
				villa_id = sendMessageReceiver.Villa_ID;
				room_id = sendMessageReceiver.Room_ID;
				break;
			case EventType.CreateRobot:
				receiver = new CreateRobotReceiver(eventData!["CreateRobot"]!.ToString());
				var createRobotReceiver = (CreateRobotReceiver)receiver;
				villa_id = createRobotReceiver.Villa_ID;
				room_id = 0;
				break;
			case EventType.DeleteRobot:
				receiver = new DeleteRobotReceiver(eventData!["DeleteRobot"]!.ToString());
				var deleteRobotReceiver = (DeleteRobotReceiver)receiver;
				villa_id = deleteRobotReceiver.Villa_ID;
				room_id = 0;
				break;
			case EventType.AddQuickEmoticon:
				receiver = new AddQuickEmoticonReceiver(eventData!["AddQuickEmoticon"]!.ToString());
				var addQuickEmoticonReceiver = (AddQuickEmoticonReceiver)receiver;
				villa_id = addQuickEmoticonReceiver.Villa_ID;
				room_id = addQuickEmoticonReceiver.Room_ID;
				break;
			case EventType.AuditCallback:
				receiver = new AuditCallbackReceiver(eventData!["AuditCallback"]!.ToString());
				var auditCallbackReceiver = (AuditCallbackReceiver)receiver;
				villa_id = auditCallbackReceiver.Villa_ID;
				room_id = auditCallbackReceiver.Room_ID;
				break;
			case EventType.ClickMsgComponent:
				receiver = new ClickMsgComponentReceiver(eventData!["ClickMsgComponent"]!.ToString());
				var clickMsgComponentReceiver = (ClickMsgComponentReceiver)receiver;
				villa_id = clickMsgComponentReceiver.Villa_ID;
				room_id = clickMsgComponentReceiver.Room_id;
				break;
		}
		receiver!.robot = this.robot;
		receiver.EventType = this.EventType;
		receiver.room_id = this.room_id;
		receiver.villa_id = this.villa_id;
	}
	internal virtual void Initialize(vila_bot.RobotEventMessage robotEventMessage)
	{
		robot = JsonConvert.DeserializeObject<Robot>(JsonConvert.SerializeObject(robotEventMessage.Event.Robot), new JsonSerializerSettings
		{
			ContractResolver = new DefaultContractResolver()
			{
				NamingStrategy = new OriginalCaseNamingStrategy()
			}// 使用 ProtoContractResolver 保持原始字段名
		});

		//var json = JObject.Parse(message)["event"];
		//robot = JsonConvert.DeserializeObject<Robot>(json!["robot"]!.ToString())!;
		EventType = (EventType)robotEventMessage.Event.Type;
		//EventType = (EventType)(int)json["type"]!;

		//var eventData = json["extend_data"]!["EventData"];
		var eventData = JObject.Parse(JsonConvert.SerializeObject(robotEventMessage.Event.ExtendData, new JsonSerializerSettings
		{
			ContractResolver = new DefaultContractResolver()
			{
				NamingStrategy = new OriginalCaseNamingStrategy()
			}// 使用 ProtoContractResolver 保持原始字段名
		}));
		//对事件数据赋值
		switch (EventType)
		{
			case EventType.JoinVilla:
				receiver = new JoinVillaReceiver(eventData!["join_villa"]!.ToString());
				var joinVillaReceiver = (JoinVillaReceiver)receiver;
				villa_id = joinVillaReceiver.villa_id;
				room_id = 0;
				break;
			case EventType.SendMessage:
				receiver = new SendMessageReceiver(eventData!["send_message"]!.ToString());
				var sendMessageReceiver = (SendMessageReceiver)receiver;
				villa_id = sendMessageReceiver.Villa_ID;
				room_id = sendMessageReceiver.Room_ID;
				break;
			case EventType.CreateRobot:
				receiver = new CreateRobotReceiver(eventData!["create_robot"]!.ToString());
				var createRobotReceiver = (CreateRobotReceiver)receiver;
				villa_id = createRobotReceiver.Villa_ID;
				room_id = 0;
				break;
			case EventType.DeleteRobot:
				receiver = new DeleteRobotReceiver(eventData!["delete_robot"]!.ToString());
				var deleteRobotReceiver = (DeleteRobotReceiver)receiver;
				villa_id = deleteRobotReceiver.Villa_ID;
				room_id = 0;
				break;
			case EventType.AddQuickEmoticon:
				receiver = new AddQuickEmoticonReceiver(eventData!["add_quick_emoticon"]!.ToString());
				var addQuickEmoticonReceiver = (AddQuickEmoticonReceiver)receiver;
				villa_id = addQuickEmoticonReceiver.Villa_ID;
				room_id = addQuickEmoticonReceiver.Room_ID;
				break;
			case EventType.AuditCallback:
				receiver = new AuditCallbackReceiver(eventData!["audit_callback"]!.ToString());
				var auditCallbackReceiver = (AuditCallbackReceiver)receiver;
				villa_id = auditCallbackReceiver.Villa_ID;
				room_id = auditCallbackReceiver.Room_ID;
				break;
			case EventType.ClickMsgComponent:
				receiver = new ClickMsgComponentReceiver(eventData!["click_msg_component"]!.ToString());
				var clickMsgComponentReceiver = (ClickMsgComponentReceiver)receiver;
				villa_id = clickMsgComponentReceiver.Villa_ID;
				room_id = clickMsgComponentReceiver.Room_id;
				break;
		}
		receiver!.robot = this.robot;
		receiver.EventType = this.EventType;
		receiver.room_id = this.room_id;
		receiver.villa_id = this.villa_id;
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
	ClickMsgComponent = 7,
}
class OriginalCaseNamingStrategy : NamingStrategy
{
	protected override string ResolvePropertyName(string name)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < name.Length; i++)
		{
			if (65 <= name[i] && name[i] <= 90)
			{
				if (i != 0)
				{
					stringBuilder.Append($"_");
				}
				stringBuilder.Append((char)(name[i] + 32));
			}
			else
			{
				stringBuilder.Append($"{name[i]}");
			}
		}
		return stringBuilder.ToString();
	}
}