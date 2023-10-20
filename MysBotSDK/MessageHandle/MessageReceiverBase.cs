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

namespace MysBotSDK.MessageHandle;

public class MessageReceiverBase
{
	/// <summary>
	/// robot信息
	/// </summary>
	public Robot robot { get; set; }

	/// <summary>
	/// 事件类型
	/// </summary>
	public EventType EventType { get; private set; }

	/// <summary>
	/// 接收器
	/// </summary>
	internal MessageReceiverBase receiver { get; set; }

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
		robot = JsonConvert.DeserializeObject<Robot>(json["robot"].ToString());
		EventType = (EventType)(int)json["type"];

		var eventData = json["extend_data"]["EventData"];

		//对事件数据赋值
		switch (EventType)
		{
			case EventType.JoinVilla:
				receiver = new JoinVillaReceiver(eventData!["JoinVilla"]!.ToString());
				break;
			case EventType.SendMessage:
				receiver = new SendMessageReceiver(eventData!["SendMessage"]!.ToString());
				break;
			case EventType.CreateRobot:
				receiver = new CreateRobotReceiver(eventData!["CreateRobot"]!.ToString());
				break;
			case EventType.DeleteRobot:
				receiver = new DeleteRobotReceiver(eventData!["DeleteRobot"]!.ToString());
				break;
			case EventType.AddQuickEmoticon:
				receiver = new AddQuickEmoticonReceiver(eventData!["AddQuickEmoticon"]!.ToString());
				break;
			case EventType.AuditCallback:
				receiver = new AuditCallbackReceiver(eventData!["AuditCallback"]!.ToString());
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