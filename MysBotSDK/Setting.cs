using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK;
public class Setting
{
	const string mys_domain = "https://bbs-api.miyoushe.com";

	public const string SendMessage = mys_domain + "/vila/api/bot/platform/sendMessage";

	public const string GetUserInformation = mys_domain + "/vila/api/bot/platform/getMember";
}
