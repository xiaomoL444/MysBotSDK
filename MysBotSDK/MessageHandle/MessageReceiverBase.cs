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
using MysBotSDK.Tool;

namespace MysBotSDK.MessageHandle;

/// <summary>
/// 消息接收器父类
/// </summary>
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

	/// <summary>
	/// 获取相应的扩展数据
	/// </summary>
	/// <typeparam name="T">数据类型</typeparam>
	/// <param name="bodyData">消息体(应是一段最初收到的json消息)</param>
	/// <returns></returns>
	internal T GetExtendDataMsg<T>(string bodyData)
	{
		var json = (JObject)JObject.Parse(bodyData)["event"]!["extend_data"]!;
		if (json.ContainsKey("EventData"))
		{
			//原格式解析
			return JsonConvert.DeserializeObject<T>(JObject.Parse(bodyData)["event"]!["extend_data"]!["EventData"]![typeof(T).Name]!.ToString())!;
		}
		else if (json.ContainsKey("event_data"))
		{
			//ws将之从下划线改成大写
			return JsonConvert.DeserializeObject<T>(JObject.Parse(bodyData)["event"]!["extend_data"]!["event_data"]![BigCamelToUnderscore(typeof(T).Name)]!.ToString())!;
		}
		else
		{
			Logger.LogError("无效的消息体");
			return default(T)!;
		}
	}
	internal Dictionary<string, string> BigCamelToUndderscore { get; set; } = new Dictionary<string, string>();
	internal string BigCamelToUnderscore(string score)
	{
		if (!BigCamelToUndderscore.ContainsKey(score))
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < score.Length; i++)
			{
				if ('A' <= score[i] && score[i] <= 'Z')
				{
					if (i != 0)
					{
						stringBuilder.Append('_');
					}
					stringBuilder.Append((char)(score[i] + 32));
					continue;
				}
				stringBuilder.Append(score[i]);
			}
			BigCamelToUndderscore[score] = stringBuilder.ToString();
		}

		return BigCamelToUndderscore[score];
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