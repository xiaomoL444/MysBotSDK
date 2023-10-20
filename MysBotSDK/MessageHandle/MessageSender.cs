using MysBotSDK.MessageHandle.Info;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Cryptography;

namespace MysBotSDK.MessageHandle;

public static class MessageSender
{
	internal static string header { get; set; } = "";//没有x-rpc-bot_villa_id
	internal static string FormatHeader(UInt64 villa_id) { return header + $"\nx-rpc-bot_villa_id:{villa_id}"; }
	private static MysBot? mysBot;
	internal static MysBot MysBot
	{
		set
		{
			mysBot = value;
			header = @$"x-rpc-bot_id:{mysBot.bot_id}
x-rpc-bot_secret:{Authentication.HmacSHA256(mysBot.secret!, mysBot.pub_key!)}
Content-Type: application/json";
		}
	}
	#region 消息发送方法

	/// <summary>
	/// 发送一条文本消息
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="room_id">房间ID</param>
	/// <param name="msg_content">消息链(MessageChain)</param>
	/// <returns></returns>
	public static async Task<(string message, int retcode, string bot_msg_id)> SendText(UInt64 villa_id, UInt64 room_id, MessageChain msg_content)
	{
		await msg_content.Bulid();

		MsgContentInfo msgContentInfo = new MsgContentInfo();
		string object_name = "MHY:Text";
		msgContentInfo.content = new MsgContent() { text = msg_content.text_, entities = msg_content.entities_ };
		msgContentInfo.mentionedInfo = new MentionedInfo() { type = msg_content.MentionType, userIdList = msg_content.entities_.Where(q => q.entity.type == Entity_Detail.EntityType.mentioned_user).Select(q => q.entity.user_id).ToList() };
		msgContentInfo.quote = msg_content.quote;

		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.SendMessage);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));

		httpRequestMessage.Content = JsonContent.Create(new { room_id, object_name, msg_content = JsonConvert.SerializeObject(msgContentInfo) });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug(res.Content.ReadAsStringAsync().Result);

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { bot_msg_id = "" }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, bot_msg_id = json.data.bot_msg_id };
	}

	/// <summary>
	/// 发送图片信息
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="room_id">房间ID</param>
	/// <param name="url">图片url</param>
	/// <param name="size">图片尺寸</param>
	/// <param name="file_size">图片大小</param>
	/// <returns></returns>
	public static async Task<(string message, int retcode, string bot_msg_id)> SendImage(UInt64 villa_id, UInt64 room_id, string url, PicContentInfo.Size size = null!, int file_size = 0)
	{
		string object_name = "MHY:Image";
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.SendMessage);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));

		httpRequestMessage.Content = JsonContent.Create(new { villa_id, room_id, object_name, msg_content = JsonConvert.SerializeObject(new { content = new PicContentInfo() { url = url, size = size, file_size = file_size } }) });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug(res.Content.ReadAsStringAsync().Result);

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { bot_msg_id = "" }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, bot_msg_id = json.data.bot_msg_id };
	}

	/// <summary>
	/// 发送帖子消息
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="room_id">房间ID</param>
	/// <param name="post_id">帖子ID</param>
	/// <returns></returns>
	public static async Task<(string message, int retcode, string bot_msg_id)> SendPost(UInt64 villa_id, UInt64 room_id, string post_id)
	{
		string object_name = "MHY:Post";
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.SendMessage);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));

		httpRequestMessage.Content = JsonContent.Create(new { villa_id, room_id, object_name, msg_content = JsonConvert.SerializeObject(new { content = new { post_id = post_id } }) });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug(res.Content.ReadAsStringAsync().Result);

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { bot_msg_id = "" }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, bot_msg_id = json.data.bot_msg_id };
	}

	/// <summary>
	/// 撤回消息
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="room_id">房间ID</param>
	/// <param name="msg_uid">消息uid</param>
	/// <param name="msg_time">消息发送时间</param>
	/// <returns></returns>
	public static async Task<(string message, int retcode, string bot_msg_id)> RecallMessage(UInt64 villa_id, UInt64 room_id, string msg_uid, Int64 msg_time)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.RecallMessage);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));

		httpRequestMessage.Content = JsonContent.Create(new { room_id, msg_time, msg_uid });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug(res.Content.ReadAsStringAsync().Result);

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { bot_msg_id = "" }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, bot_msg_id = json.data.bot_msg_id };
	}

	/// <summary>
	/// 置顶消息
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="room_id">房间ID</param>
	/// <param name="msg_uid">消息uid</param>
	/// <param name="msg_time">消息发送时间</param>
	/// <param name="is_cancel">是否取消置顶消息</param>
	/// <returns></returns>
	public static async Task<(string message, int retcode, string bot_msg_id)> PinMessage(UInt64 villa_id, UInt64 room_id, string msg_uid, Int64 msg_time, bool is_cancel)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.PinMessage);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));

		httpRequestMessage.Content = JsonContent.Create(new { room_id, msg_time, msg_uid, is_cancel });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug(res.Content.ReadAsStringAsync().Result);

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { bot_msg_id = "" }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, bot_msg_id = json.data.bot_msg_id };
	}

	/// <summary>
	/// 创建大别野分组
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="group_name">组别名字</param>
	/// <returns></returns>
	public static async Task<(string message, int retcode, string group_id)> CreateGroup(UInt64 villa_id, string group_name)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.CreateGroup);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));

		httpRequestMessage.Content = JsonContent.Create(new { group_name });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug(res.Content.ReadAsStringAsync().Result);

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { group_id = "" }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, group_id = json.data.group_id };
	}

	/// <summary>
	/// 删除分组
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="group_id">组别ID</param>
	/// <returns></returns>
	public static async Task<(string message, int retcode)> DeleteGroup(UInt64 villa_id, UInt64 group_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.DeleteGroup);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));

		httpRequestMessage.Content = JsonContent.Create(new { group_id });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug(res.Content.ReadAsStringAsync().Result);

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode };
	}
	public static async Task<(string message, int retcode)> EditGroup(UInt64 villa_id, UInt64 group_id, string new_group_name)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.EditGroup);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));

		httpRequestMessage.Content = JsonContent.Create(new { group_id, group_name = new_group_name });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug(res.Content.ReadAsStringAsync().Result);

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode };
	}
	public static async Task<(string message, int retcode)> EditRoom(UInt64 villa_id, UInt64 room_id, string new_room_name)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.EditRoom);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));

		httpRequestMessage.Content = JsonContent.Create(new { room_id, room_name = new_room_name });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug(res.Content.ReadAsStringAsync().Result);

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode };
	}
	public static async Task<(string message, int retcode)> DeleteRoom(UInt64 villa_id, UInt64 room_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.DeleteRoom);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));

		httpRequestMessage.Content = JsonContent.Create(new { room_id });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug(res.Content.ReadAsStringAsync().Result);

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode };
	}
	#endregion
	#region API获取信息
	/// <summary>
	/// 获取用户信息
	/// </summary>
	/// <param name="villa_id">大别野id</param>
	/// <param name="uid">用户id</param>
	/// <returns></returns>
	public static async Task<(string message, int retcode, Member member)> GetUserInfo(UInt64 villa_id, UInt64 uid)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetUserInfo);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { uid });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取用户信息{res.Content.ReadAsStringAsync().Result}");

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { member = new Member() }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, member = json.data.member };
	}
	public static async Task<(string message, int retcode, Villa villa)> GetVillaInfo(UInt64 villa_id)
	{

		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetVillaInfo);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { villa_id = villa_id });

		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取大别野信息{res.Content.ReadAsStringAsync().Result}");
		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { villa = new Villa() }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, villa = json.data.villa };
	}
	public static async Task<(string message, int retcode, Room room)> GetRoomInfo(UInt64 villa_id, UInt64 room_id)
	{

		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetRoomInfo);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { room_id = room_id });

		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取房间信息{res.Content.ReadAsStringAsync().Result}");
		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { room = new Room() }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, room = json.data.room };
	}
	public static async Task<(string message, int retcode, List<Member> members)> GetVillaMember(UInt64 villa_id)
	{
		int size_count = 10;
		int result_count = 0;
		string message = string.Empty;
		int retcode = 0;
		List<Member> members = new List<Member>();
		do
		{
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetVillaMember);
			httpRequestMessage.AddHeaders(FormatHeader(villa_id));
			httpRequestMessage.Content = JsonContent.Create(new { size = 10, offset_str = "" });

			var res = await HttpClass.SendAsync(httpRequestMessage);
			Logger.Debug($"获取大别野成员信息{res.Content.ReadAsStringAsync().Result}");
			var AnonymousType = new
			{
				retcode = 0,
				message = "",
				data = new { next_offset_str = "", list = new List<Member>() }
			};
			var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
			retcode = json!.retcode;
			message = json.message;

			result_count = json.data.list.Count;
			members.AddRange(json.data.list);
		} while (result_count == size_count && retcode == 0);

		return new() { message = message, retcode = retcode, members = members };
	}
	public static async Task<(string message, int retcode, List<Group> groups)> GetGroupList(UInt64 villa_id)
	{

		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetGroupList);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		//httpRequestMessage.Content = JsonContent.Create(new { room_id = room_id });

		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取大别野分组列表信息{res.Content.ReadAsStringAsync().Result}");
		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { list = new List<Group>() }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, groups = json.data.list };
	}
	public static async Task<(string message, int retcode, List<Room> rooms)> GetRoomList(UInt64 villa_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetRoomList);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		//httpRequestMessage.Content = JsonContent.Create(new { room_id = room_id });

		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取房间列表信息{res.Content.ReadAsStringAsync().Result}");
		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { list = new List<Room>() }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, rooms = json.data.list };
	}
	public static async Task<(string message, int retcode, BotMemberAccessInfo access_info, Member member)> CheckMemberBotAccessToken(UInt64 villa_id, string token)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.CheckMemberBotAccessToken);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { token });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取用户凭证{res.Content.ReadAsStringAsync().Result}");

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { access_info = new BotMemberAccessInfo(), member = new Member() }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, access_info = json.data.access_info, member = json.data.member };
	}
	public static async Task<(string message, int retcode, List<Emoticon> emoticons)> GetAllEmoticons(UInt64 villa_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetAllEmoticon);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));

		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取表情{res.Content.ReadAsStringAsync().Result}");
		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { list = new List<Emoticon>() }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, emoticons = json.data.list };
	}
	#endregion
	#region 身份组
	public static async Task<(string message, int retcode)> OperateMemberToRole(UInt64 villa_id, UInt64 user_id, UInt64 role_id, bool is_add)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.OperateMemberToRole);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { uid = user_id, role_id, is_add });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取向身份组添加用户{res.Content.ReadAsStringAsync().Result}");

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode };
	}
	public static async Task<(string message, int retcode, string id)> CreateMemberRole(UInt64 villa_id, string name, string color, Permission permission)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.CreateMemberRole);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { name, color, permissions = permission.ToString().Split(",") });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"创建身份组{res.Content.ReadAsStringAsync().Result}");

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { id = "" }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, id = json.data.id };
	}
	public static async Task<(string message, int retcode)> EditMemberRole(UInt64 villa_id, UInt64 id, string new_name, string new_color, Permission new_permission)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.EditMemberRole);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { id, name = new_name, color = new_color, permissions = new_permission.ToString().Split(",") });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"编辑身份组{res.Content.ReadAsStringAsync().Result}");

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode };
	}
	public static async Task<(string message, int retcode)> DeleteMemberRole(UInt64 villa_id, UInt64 id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.DeleteMemberRole);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { id });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"删除身份组{res.Content.ReadAsStringAsync().Result}");

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode };
	}
	public static async Task<(string message, int retcode, MemberRole member_role)> GetVillaMemberRoleInfo(UInt64 villa_id, UInt64 role_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetVillaMemberRoleInfo);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { role_id });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取身份组{res.Content.ReadAsStringAsync().Result}");

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { role = new MemberRole() }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, member_role = json.data.role };
	}
	public static async Task<(string message, int retcode, List<MemberRole> member_roles)> GetVillaMemberRoleList(UInt64 villa_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetVillaMemberRole);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));

		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取身份组列表{res.Content.ReadAsStringAsync().Result}");

		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { list = new List<MemberRole>() }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, member_roles = json.data.list };
	}
	#endregion
	#region 审核
	public static async Task<(string message, int retcode)> Audit(UInt64 villa_id, string audit_content, UInt64 uid, Content_Type content_type, string pass_through = "", UInt64 room_id = 0)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.Audit);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { audit_content, uid, content_type = Enum.GetName(typeof(Content_Type), content_type), pass_through, room_id });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取表情{res.Content.ReadAsStringAsync().Result}");
		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode };
	}
	#endregion
	#region 图片转存
	public static async Task<(string message, int retcode, string new_url)> Transferimage(UInt64 villa_id, string url)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.TransferImage);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { url });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"转换床图{res.Content.ReadAsStringAsync().Result}");
		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { new_url = "" }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);
		return new() { message = json!.message, retcode = json.retcode, new_url = json.data.new_url };
	}
	#endregion
}