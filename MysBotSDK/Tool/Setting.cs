using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.Tool
{
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

		public const string GetUserInfo = mys_domain + "/vila/api/bot/platform/getMember";//Get
		public const string GetVillaInfo = mys_domain + "/vila/api/bot/platform/getVilla";//Gei
		public const string GetRoomInfo = mys_domain + "/vila/api/bot/platform/getRoom";//Get
		public const string GetVillaMember = mys_domain + "/vila/api/bot/platform/getVillaMembers";//Get
		public const string GetGroupList = mys_domain + "/vila/api/bot/platform/getGroupList";//Get
		public const string GetRoomList = mys_domain + "/vila/api/bot/platform/getVillaGroupRoomList";//Get

		public const string CheckMemberBotAccessToken = mys_domain + "/vila/api/bot/platform/checkMemberBotAccessToken";//Get

		public const string OperateMemberToRole = mys_domain + "/vila/api/bot/platform/operateMemberToRole";//Post
		public const string CreateMemberRole = mys_domain + "/vila/api/bot/platform/createMemberRole";//Post
		public const string EditMemberRole = mys_domain + "/vila/api/bot/platform/editMemberRole";//Post
		public const string DeleteMemberRole = mys_domain + "/vila/api/bot/platform/deleteMemberRole";//Post

		public const string GetVillaMemberRoleInfo = mys_domain + "/vila/api/bot/platform/getMemberRoleInfo";//Get
		public const string GetVillaMemberRole = mys_domain + "/vila/api/bot/platform/getVillaMemberRoles";//Get

		public const string GetAllEmoticon = mys_domain + "/vila/api/bot/platform/getAllEmoticons";//Get

		public const string Audit = mys_domain + "/vila/api/bot/platform/audit";//Post

		public const string TransferImage = mys_domain + "/vila/api/bot/platform/transferImage";//Post
		public const string UploadImage = mys_domain + "/vila/api/bot/platform/getUploadImageParams";//Get

		public const string DeleteVillaMember = mys_domain + "/vila/api/bot/platform/deleteVillaMember";//Post  

		public const string GetWebSocketInfo = mys_domain + "/vila/api/bot/platform/getWebsocketInfo";//Get http://devapi-takumi.mihoyo.com
	}
}