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
	/// WebHook构造器
	/// </summary>
	/// <param name="message"></param>
	public MessageReceiverBase(string message)
	{
		//解析ExtendData以外的内容
		var json = JObject.Parse(message)["event"];
		robot = JsonConvert.DeserializeObject<Robot>(json!["robot"]!.ToString())!;
		EventType = (EventType)(int)json["type"]!;
		//至于roomid与villaid则在子类构造器里实现

	}

	internal T GetExtendDataMsg<T>(string bodyData)
	{
		return (JsonConvert.DeserializeObject<T>(JObject.Parse(bodyData)["event"]!["extend_data"]!["EventData"]![typeof(T).Name]!.ToString()));
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