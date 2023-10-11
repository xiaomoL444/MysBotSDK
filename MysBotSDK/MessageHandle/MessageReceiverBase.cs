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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MysBotSDK.MessageHandle;

public class MessageReceiverBase
{
	public Robot robot { get; set; }
	public EventType EventType { get; private set; }
	internal MessageReceiverBase receiver { get; set; }

	public MessageReceiverBase(string message)
	{
		Initialize(message);
	}
	public virtual void Initialize(string message)
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
public enum EventType
{
	JoinVilla = 1,
	SendMessage = 2,
	CreateRobot = 3,
	DeleteRobot = 4,
	AddQuickEmoticon = 5,
	AuditCallback = 6,
}

public class JoinVillaReceiver : MessageReceiverBase
{
	public JoinVilla? joinVilla { get; set; }
	public JoinVillaReceiver(string message) : base(message)
	{

	}
	public override void Initialize(string message)
	{
		joinVilla = JsonConvert.DeserializeObject<JoinVilla>(message)!;
	}
}
public class SendMessageReceiver : MessageReceiverBase
{
	public SendMessage? sendMessage { get; set; }
	public List<string> args => sendMessage.args;

	public SendMessageReceiver(string message) : base(message)
	{
	}
	public override void Initialize(string message)
	{
		sendMessage = JsonConvert.DeserializeObject<SendMessage>(message)!;
		var args = sendMessage!.content.content.text.Split(" ").ToList();
		args.RemoveRange(0, 2);
		sendMessage.args = args;
	}
}
public class CreateRobotReceiver : MessageReceiverBase
{
	public CreateRobot? createRobot { get; set; }
	public CreateRobotReceiver(string message) : base(message)
	{
	}
	public override void Initialize(string message)
	{
		createRobot = JsonConvert.DeserializeObject<CreateRobot>(message)!;
	}
}
public class DeleteRobotReceiver : MessageReceiverBase
{
	public DeleteRobot? deleteRobot { get; set; }
	public DeleteRobotReceiver(string message) : base(message)
	{
	}
	public override void Initialize(string message)
	{
		deleteRobot = JsonConvert.DeserializeObject<DeleteRobot>(message)!;
	}
}
public class AddQuickEmoticonReceiver : MessageReceiverBase
{
	public AddQuickEmoticon addQuickEmoticon { get; set; }
	public AddQuickEmoticonReceiver(string message) : base(message)
	{
	}
	public override void Initialize(string message)
	{
		addQuickEmoticon = JsonConvert.DeserializeObject<AddQuickEmoticon>(message)!;
	}
}
public class AuditCallbackReceiver : MessageReceiverBase
{
	public AuditCallback? auditCallback { get; set; }
	public AuditCallbackReceiver(string message) : base(message)
	{
	}
	public override void Initialize(string message)
	{
		auditCallback = JsonConvert.DeserializeObject<AuditCallback>(message);
	}
}

