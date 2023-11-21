using Google.Protobuf;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VilaBot;

namespace MysBotSDK
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			Logger.Log("It's a Program");
			string str = @"{""event"":{""robot"":{""template"":{""id"":""bot_w56csMVkMDa5efrcv7mg"",""name"":""末醬映射機"",""desc"":""是先有末酱再有晓末(生气)"",""icon"":""https://upload-bbs.miyoushe.com/upload/2023/10/15/257585750/219ebfe40777cfb4f4007b6d583ab59b_786924592831393165.jpg"",""commands"":[{""name"":""/Test"",""desc"":""...继承了上一个测试""},{""name"":""/魔法札记"",""desc"":""测试魔法札记功能""},{""name"":""/解析"",""desc"":""解析json并以Text文本发送""},{""name"":""/设置面板"",""desc"":""定时器设置面板""},{""name"":""/猜拳"",""desc"":""猜拳test""}]},""villa_id"":8489},""type"":2,""extend_data"":{""EventData"":{""SendMessage"":{""content"":""{\""content\"":{\""text\"":\""@末醬映射機 /猜拳 \"",\""entities\"":[{\""offset\"":0,\""length\"":7,\""entity\"":{\""type\"":\""mentioned_robot\"",\""bot_id\"":\""bot_w56csMVkMDa5efrcv7mg\""}}]},\""user\"":{\""id\"":\""257585750\"",\""portraitUri\"":\""https://bbs-static.miyoushe.com/avatar/avatar30024.png\"",\""alias\"":\""\"",\""extra\"":\""{\\\""member_roles\\\"":{\\\""name\\\"":\\\""月下🌙孤徜\\\"",\\\""color\\\"":\\\""#6173AB\\\"",\\\""web_color\\\"":\\\""#C6C9D1\\\"",\\\""role_font_color\\\"":\\\""#6173AB\\\"",\\\""role_bg_color\\\"":\\\""#E3E7F4\\\"",\\\""color_scheme_id\\\"":1},\\\""state\\\"":null}\"",\""portrait\"":\""https://bbs-static.miyoushe.com/avatar/avatar30024.png\"",\""name\"":\""曉末L444\""},\""mentionedInfo\"":{\""mentionedContent\"":\""[@我] 曉末L444：@末醬映射機 /猜拳 \"",\""type\"":2,\""userIdList\"":[\""bot_w56csMVkMDa5efrcv7mg\""]},\""panel\"":{\""template_id\"":0,\""group_list\"":[],\""small_component_group_list\"":null,\""mid_component_group_list\"":null,\""big_component_group_list\"":null}}"",""from_user_id"":257585750,""send_at"":1700478465791,""object_name"":1,""room_id"":129608,""nickname"":""曉末L444"",""msg_uid"":""CBTH-L3DV-VHN9-KFNT"",""villa_id"":8489}}},""created_at"":1700478465,""id"":""1563b482-d4cc-3d21-b4a5-a0e72dbceef0"",""send_at"":1700478466}}";
			VilaBot.RobotEventMessage robotEventMessage = RobotEventMessage.Parser.ParseJson(str);
			JsonFormatter.Default.Format(robotEventMessage);
		}
	}
}
