using Google.Protobuf;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.ExtendData;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Text;
using vila_bot;

namespace MysBotSDK;

internal class Program
{
	public static void Main(string[] args)
	{
		var packMsg = new
		{
			@event = new
			{
				robot = new Robot() { Template = new(), VillaId = new() },
				type = RobotEvent.Types.EventType.JoinVilla,
				extend_data = new
				{
					EventData = new RobotEvent.Types.ExtendData()
				},
				create_at = "123",
				id = 123,
				send_at = "123"
			}
		};

		//var json = JsonFormatter.Default.Format(robotEvent);//尝试将Protobuf消息转化成Protobuf消息
		Logger.Log(JsonConvert.SerializeObject(packMsg));
	}
}
