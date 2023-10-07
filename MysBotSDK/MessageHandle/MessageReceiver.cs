using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MysBotSDK.MessageHandle;

public class MessageReceiver
{
	public Robot robot { get; set; }
	public EventType EventType { get; private set; }
	public JoinVilla JoinVilla { get; private set; }
	public SendMessage SendMessage { get; private set; }
	public CreateRobot CreateRobot { get; private set; }
	public DeleteRobot DeleteRobot { get; private set; }
	public AddQuickEmoticon AddQuickEmoticon { get; private set; }
	public AuditCallback AuditCallback { get; private set; }
	public MessageReceiver(string message)
	{
		var json = JObject.Parse(message)["event"];
		robot = JsonConvert.DeserializeObject<Robot>(json["robot"].ToString());
		EventType = (EventType)(int)json["type"];

		var eventData = json["extend_data"]["EventData"];

		//对事件数据赋值
		switch (EventType)
		{
			case EventType.JoinVilla:
				JoinVilla = JsonConvert.DeserializeObject<JoinVilla>(eventData["JoinVilla"].ToString());
				break;
			case EventType.SendMessage:
				SendMessage = JsonConvert.DeserializeObject<SendMessage>(eventData["SendMessage"].ToString());
				var args = SendMessage.content.content.text.Split(" ").ToList();
				args.RemoveRange(0, 2);
				SendMessage.args = args;
				break;
			case EventType.CreateRobot:
				CreateRobot = JsonConvert.DeserializeObject<CreateRobot>(eventData["CreateRobot"].ToString());
				break;
			case EventType.DeleteRobot:
				DeleteRobot = JsonConvert.DeserializeObject<DeleteRobot>(eventData["DeleteRobot"].ToString());
				break;
			case EventType.AddQuickEmoticon:
				AddQuickEmoticon = JsonConvert.DeserializeObject<AddQuickEmoticon>(eventData["AddQuickEmoticon"].ToString());
				break;
			case EventType.AuditCallback:
				AuditCallback = JsonConvert.DeserializeObject<AuditCallback>(eventData["AuditCallback"].ToString());
				break;
			default:
				break;
		}
	}
}

public class Robot
{
	/// <summary>
	/// Bot信息
	/// </summary>
	public Template template { get; set; }
	/// <summary>
	/// 大别野ID
	/// </summary>
	public int villa_id { get; set; }
}
public class Template
{
	/// <summary>
	/// BotID
	/// </summary>
	public string id { get; set; }
	/// <summary>
	/// Bot名字
	/// </summary>
	public string name { get; set; }
	/// <summary>
	/// Bot描述
	/// </summary>
	public string desc { get; set; }
	/// <summary>
	/// Bot头像
	/// </summary>
	public string icon { get; set; }
	/// <summary>
	/// 用户使用的快捷命令
	/// </summary>
	public List<CommandsItem> commands { get; set; }
}
public class CommandsItem
{
	/// <summary>
	/// commond命令
	/// </summary>
	public string name { get; set; }
	/// <summary>
	/// commond描述
	/// </summary>
	public string desc { get; set; }
}

public interface IExtendData
{

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


