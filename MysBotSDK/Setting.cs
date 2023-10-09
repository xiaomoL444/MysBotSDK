using System;
using System.Collections.Generic;
using System.Globalization;
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

	public const string CreateGroup = mys_domain + "/vila/api/bot/platform/createGroup";//Post
	public const string EditGroup = mys_domain + "/vila/api/bot/platform/editGroup";//Post
	public const string DeleteGroup = mys_domain + "/vila/api/bot/platform/deleteGroup";//Post
	public const string EditRoom = mys_domain + "/vila/api/bot/platform/editRoom";//Post
	public const string DeleteRoom = mys_domain + "/vila/api/bot/platform/deleteRoom";//Post

	public const string GetUserInformation = mys_domain + "/vila/api/bot/platform/getMember";//Get
	public const string GetVillaInformation = mys_domain + "/vila/api/bot/platform/getVilla";//Gei
	public const string GetRoomInformation = mys_domain + "/vila/api/bot/platform/getRoom";//Get
	public const string GetVillaMember = mys_domain + "/vila/api/bot/platform/getVillaMembers";//Get
	public const string GetGroupList = mys_domain + "/vila/api/bot/platform/getGroupList";//Get
	public const string GetRoomList = mys_domain + "/vila/api/bot/platform/getVillaGroupRoomList";//Get

	public const string CheckMemberBotAccessToken = mys_domain + "/vila/api/bot/platform/checkMemberBotAccessToken";//Get

	public const string OperateMemberToRole = mys_domain + "/vila/api/bot/platform/operateMemberToRole";//Post

	public const string GetVillaMemberRole = mys_domain + "/vila/api/bot/platform/getVillaMemberRoles";//Get
}
