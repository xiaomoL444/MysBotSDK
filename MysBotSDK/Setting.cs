using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK;
public class Setting
{
	const string mys_domain = "https://bbs-api.miyoushe.com";

	public const string SendMessage = mys_domain + "/vila/api/bot/platform/sendMessage";//Post
	public const string RecallMessage = mys_domain + "/vila/api/bot/platform/recallMessage";//Post
	public const string PinMessage = mys_domain + "/vila/api/bot/platform/pinMessage";//Post

	public const string GetUserInformation = mys_domain + "/vila/api/bot/platform/getMember";//Get
	public const string GetVillaInformation = mys_domain + "/vila/api/bot/platform/getVilla";//Gei
	public const string GetRoomInformation = mys_domain + "/vila/api/bot/platform/getRoom";//Get

}
