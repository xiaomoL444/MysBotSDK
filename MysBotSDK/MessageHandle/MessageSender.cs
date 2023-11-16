using MysBotSDK.MessageHandle.Info;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

namespace MysBotSDK.MessageHandle;

public static class MessageSender
{
	internal static string GetHeader(MysBot mysBot)//没有x-rpc-bot_villa_id
	{
		return @$"x-rpc-bot_id:{mysBot.bot_id}
x-rpc-bot_secret:{Authentication.HmacSHA256(mysBot.secret!, mysBot.pub_key!)}
Content-Type: application/json";
	}
	internal static string FormatHeader(MysBot mysBot, UInt64 villa_id) { return GetHeader(mysBot) + $"\nx-rpc-bot_villa_id:{villa_id}"; }
	internal static List<MysBot> mysBot { get; set; } = new List<MysBot>();
	internal static MysBot MysBot
	{
		set
		{
			if (mysBot.Contains(value))
			{
				Logger.LogWarnning("重复加载Bot");
			}
			else
			{
				mysBot.Add(value);
			}
		}
	}

	#region 消息发送方法

	/// <summary>
	/// 发送一条文本消息
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="room_id">房间ID</param>
	/// <param name="msg_content">消息链(MessageChain)</param>
	/// <returns>message:返回消息,retcode:返回消息code,bot_msg_id:消息uid</returns>
	public static async Task<(string message, int retcode, string bot_msg_id)> SendText(UInt64 villa_id, UInt64 room_id, MessageChain msg_content)//默认用最新添加的bot发送消息
	{
		return await SendText(mysBot[mysBot.Count - 1], villa_id, room_id, msg_content);
	}
	public static async Task<(string message, int retcode, string bot_msg_id)> SendText(MysBot mysBot, UInt64 villa_id, UInt64 room_id, MessageChain msg_content)
	{
		//Bulid信息
		await msg_content.Bulid();

		MsgContentInfo msgContentInfo = new MsgContentInfo();
		string object_name = "MHY:Text";
		msgContentInfo.content = new MsgContent() { text = msg_content.text_, entities = msg_content.entities_, images = msg_content.images };
		//添加@At信息(必须单独添加，否则会@At所有人)
		if (msg_content.mentionType != MentionType.None)
		{
			msgContentInfo.mentionedInfo = new MentionedInfo() { type = msg_content.mentionType, userIdList = msg_content.entities_.Where(q => q.entity.type == Entity_Detail.EntityType.mentioned_user).Select(q => q.entity.user_id).ToList()! };
		}
		//添加引用信息
		msgContentInfo.quote = msg_content.quote;

		//添加组件信息
		msgContentInfo.panel!.template_id = msg_content.template_id;
		msgContentInfo.panel.small_component_group_list = msg_content.smallComponent[0].Count == 0 ? null! : msg_content.smallComponent;
		msgContentInfo.panel.mid_component_group_list = msg_content.midComponent[0].Count == 0 ? null! : msg_content.midComponent;
		msgContentInfo.panel.big_component_group_list = msg_content.bigComponent[0].Count == 0 ? null! : msg_content.bigComponent;

		//发送消息
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.SendMessage);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));

		httpRequestMessage.Content = JsonContent.Create(new
		{
			room_id,
			object_name,
			msg_content = JsonConvert.SerializeObject(msgContentInfo)
		});
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
	/// <returns>message:返回消息,retcode:返回消息code,bot_msg_id:消息uid</returns>
	public static async Task<(string message, int retcode, string bot_msg_id)> SendImage(UInt64 villa_id, UInt64 room_id, string url, PicContentInfo.Size size = null!, int file_size = 0)
	{
		return await SendImage(mysBot[mysBot.Count - 1], villa_id, room_id, url, size, file_size);
	}
	public static async Task<(string message, int retcode, string bot_msg_id)> SendImage(MysBot mysBot, UInt64 villa_id, UInt64 room_id, string url, PicContentInfo.Size size = null!, int file_size = 0)
	{
		string object_name = "MHY:Image";
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.SendMessage);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));

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
	/// <returns>message:返回消息,retcode:返回消息code,bot_msg_id:消息uid</returns>
	public static async Task<(string message, int retcode, string bot_msg_id)> SendPost(UInt64 villa_id, UInt64 room_id, string post_id)
	{
		return await SendPost(mysBot[mysBot.Count - 1], villa_id, room_id, post_id);
	}
	public static async Task<(string message, int retcode, string bot_msg_id)> SendPost(MysBot mysBot, UInt64 villa_id, UInt64 room_id, string post_id)
	{
		string object_name = "MHY:Post";
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.SendMessage);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));

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
	/// <returns>message:返回消息,retcode:返回消息code,bot_msg_id:消息uid</returns>
	public static async Task<(string message, int retcode, string bot_msg_id)> RecallMessage(UInt64 villa_id, UInt64 room_id, string msg_uid, Int64 msg_time)
	{
		return await RecallMessage(mysBot[mysBot.Count - 1], villa_id, room_id, msg_uid, msg_time);
	}
	public static async Task<(string message, int retcode, string bot_msg_id)> RecallMessage(MysBot mysBot, UInt64 villa_id, UInt64 room_id, string msg_uid, Int64 msg_time)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.RecallMessage);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));

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
	/// <returns>message:返回消息,retcode:返回消息code,bot_msg_id:消息uid</returns>
	public static async Task<(string message, int retcode, string bot_msg_id)> PinMessage(UInt64 villa_id, UInt64 room_id, string msg_uid, Int64 msg_time, bool is_cancel)
	{
		return await PinMessage(mysBot[mysBot.Count - 1], villa_id, room_id, msg_uid, msg_time, is_cancel);
	}
	public static async Task<(string message, int retcode, string bot_msg_id)> PinMessage(MysBot mysBot, UInt64 villa_id, UInt64 room_id, string msg_uid, Int64 msg_time, bool is_cancel)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.PinMessage);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));

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
	#endregion

	#region 大别野组别、房间
	/// <summary>
	/// 创建大别野分组
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="group_name">组别名字</param>
	/// <returns>message:返回消息,retcode:返回消息code,group_id:组别id</returns>
	public static async Task<(string message, int retcode, string group_id)> CreateGroup(UInt64 villa_id, string group_name)
	{
		return await CreateGroup(mysBot[mysBot.Count - 1], villa_id, group_name);
	}
	public static async Task<(string message, int retcode, string group_id)> CreateGroup(MysBot mysBot, UInt64 villa_id, string group_name)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.CreateGroup);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));

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
	/// <returns>message:返回消息,retcode:返回消息code</returns>
	public static async Task<(string message, int retcode)> DeleteGroup(UInt64 villa_id, UInt64 group_id)
	{
		return await DeleteGroup(mysBot[mysBot.Count - 1], villa_id, group_id);
	}
	public static async Task<(string message, int retcode)> DeleteGroup(MysBot mysBot, UInt64 villa_id, UInt64 group_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.DeleteGroup);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));

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

	/// <summary>
	/// 编辑组别
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="group_id">组别ID</param>
	/// <param name="new_group_name">组别新名称</param>
	/// <returns>message:返回消息,retcode:返回消息code</returns>
	public static async Task<(string message, int retcode)> EditGroup(UInt64 villa_id, UInt64 group_id, string new_group_name)
	{
		return await EditGroup(mysBot[mysBot.Count - 1], villa_id, group_id, new_group_name);
	}
	public static async Task<(string message, int retcode)> EditGroup(MysBot mysBot, UInt64 villa_id, UInt64 group_id, string new_group_name)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.EditGroup);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));

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

	/// <summary>
	/// 编辑房间
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="room_id">房间ID</param>
	/// <param name="new_room_name">房间新名称</param>
	/// <returns>message:返回消息,retcode:返回消息code</returns>
	public static async Task<(string message, int retcode)> EditRoom(UInt64 villa_id, UInt64 room_id, string new_room_name)
	{
		return await EditRoom(mysBot[mysBot.Count - 1], villa_id, room_id, new_room_name);
	}
	public static async Task<(string message, int retcode)> EditRoom(MysBot mysBot, UInt64 villa_id, UInt64 room_id, string new_room_name)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.EditRoom);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));

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

	/// <summary>
	/// 删除房间
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="room_id">房间ID</param>
	/// <returns>message:返回消息,retcode:返回消息code</returns>
	public static async Task<(string message, int retcode)> DeleteRoom(UInt64 villa_id, UInt64 room_id)
	{
		return await DeleteRoom(mysBot[mysBot.Count - 1], villa_id, room_id);
	}
	public static async Task<(string message, int retcode)> DeleteRoom(MysBot mysBot, UInt64 villa_id, UInt64 room_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.DeleteRoom);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));

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
	/// <param name="villa_id">大别野ID</param>
	/// <param name="uid">用户UID</param>
	/// <returns>message:返回消息,retcode:返回消息code,member:Member类消息</returns>	
	public static async Task<(string message, int retcode, Member member)> GetUserInfo(UInt64 villa_id, UInt64 uid)
	{
		return await GetUserInfo(mysBot[mysBot.Count - 1], villa_id, uid);
	}
	public static async Task<(string message, int retcode, Member member)> GetUserInfo(MysBot mysBot, UInt64 villa_id, UInt64 uid)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetUserInfo);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 获得大别野信息
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <returns>message:返回消息,retcode:返回消息code,villa:Villa类消息</returns>	
	public static async Task<(string message, int retcode, Villa villa)> GetVillaInfo(UInt64 villa_id)
	{
		return await GetVillaInfo(mysBot[mysBot.Count - 1], villa_id);
	}
	public static async Task<(string message, int retcode, Villa villa)> GetVillaInfo(MysBot mysBot, UInt64 villa_id)
	{

		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetVillaInfo);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 获取房间信息
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="room_id">房间ID</param>
	/// <returns>message:返回消息,retcode:返回消息code,room:Room类消息</returns>
	public static async Task<(string message, int retcode, Room room)> GetRoomInfo(UInt64 villa_id, UInt64 room_id)
	{
		return await GetRoomInfo(mysBot[mysBot.Count - 1], villa_id, room_id);
	}
	public static async Task<(string message, int retcode, Room room)> GetRoomInfo(MysBot mysBot, UInt64 villa_id, UInt64 room_id)
	{

		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetRoomInfo);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 获取大别野所有用户
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <returns>message:返回消息,retcode:返回消息code,members:Member类列表消息</returns>
	public static async Task<(string message, int retcode, List<Member> members)> GetVillaMember(UInt64 villa_id)
	{
		return await GetVillaMember(mysBot[mysBot.Count - 1], villa_id);
	}
	public static async Task<(string message, int retcode, List<Member> members)> GetVillaMember(MysBot mysBot, UInt64 villa_id)
	{
		int size_count = 10;
		int result_count = 0;
		string message = string.Empty;
		int retcode = 0;
		List<Member> members = new List<Member>();
		do
		{
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetVillaMember);
			httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 获取所有组别
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <returns>message:返回消息,retcode:返回消息code,groups:组别列表消息</returns>
	public static async Task<(string message, int retcode, List<Group> groups)> GetGroupList(UInt64 villa_id)
	{
		return await GetGroupList(mysBot[mysBot.Count - 1], villa_id);
	}
	public static async Task<(string message, int retcode, List<Group> groups)> GetGroupList(MysBot mysBot, UInt64 villa_id)
	{

		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetGroupList);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 获取所有房间
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <returns>message:返回消息,retcode:返回消息code,rooms:Room类型列表消息</returns>	
	public static async Task<(string message, int retcode, List<Room> rooms)> GetRoomList(UInt64 villa_id)
	{
		return await GetRoomList(mysBot[mysBot.Count - 1], villa_id);
	}
	public static async Task<(string message, int retcode, List<Room> rooms)> GetRoomList(MysBot mysBot, UInt64 villa_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetRoomList);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 校验用户机器人访问凭证
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="token">token</param>
	/// <returns>message:返回消息,retcode:返回消息code,access_info:BotMemberAccessInfo类型消息,member:Member类型消息</returns>
	public static async Task<(string message, int retcode, BotMemberAccessInfo access_info, Member member)> CheckMemberBotAccessToken(UInt64 villa_id, string token)
	{
		return await CheckMemberBotAccessToken(mysBot[mysBot.Count - 1], villa_id, token);
	}
	public static async Task<(string message, int retcode, BotMemberAccessInfo access_info, Member member)> CheckMemberBotAccessToken(MysBot mysBot, UInt64 villa_id, string token)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.CheckMemberBotAccessToken);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 获得所有表情
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <returns>message:返回消息,retcode:返回消息code,emoticons:Emoticon类型列表消息</returns>
	public static async Task<(string message, int retcode, List<Emoticon> emoticons)> GetAllEmoticons(UInt64 villa_id)
	{
		return await GetAllEmoticons(mysBot[mysBot.Count - 1], villa_id);
	}
	public static async Task<(string message, int retcode, List<Emoticon> emoticons)> GetAllEmoticons(MysBot mysBot, UInt64 villa_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetAllEmoticon);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));

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

	#region 踢出用户

	/// <summary>
	/// 提出用户
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="user_id">用户uid</param>
	/// <returns></returns>
	public static async Task<(string message, int retcode)> DeleteVillaMember(UInt64 villa_id, UInt64 user_id)
	{
		return await DeleteVillaMember(mysBot[mysBot.Count - 1], villa_id, user_id);
	}
	public static async Task<(string message, int retcode)> DeleteVillaMember(MysBot mysBot, UInt64 villa_id, UInt64 user_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.DeleteVillaMember);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { uid = user_id });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"踢出用户{res.Content.ReadAsStringAsync().Result}");

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

	#region 身份组

	/// <summary>
	/// 更改用户身份组信息
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="user_id">房间ID</param>
	/// <param name="role_id">身份组ID</param>
	/// <param name="is_add">是否是添加身份组</param>
	/// <returns>message:返回消息,retcode:返回消息code</returns>
	public static async Task<(string message, int retcode)> OperateMemberToRole(UInt64 villa_id, UInt64 user_id, UInt64 role_id, bool is_add)
	{
		return await OperateMemberToRole(mysBot[mysBot.Count - 1], villa_id, user_id, role_id, is_add);
	}
	public static async Task<(string message, int retcode)> OperateMemberToRole(MysBot mysBot, UInt64 villa_id, UInt64 user_id, UInt64 role_id, bool is_add)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.OperateMemberToRole);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 创建身份组
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="name">身份组名字</param>
	/// <param name="color">身份组颜色</param>
	/// <param name="permission">身份组权限(可通过+=添加权限)</param>
	/// <returns>message:返回消息,retcode:返回消息code,id:身份组ID</returns>	
	public static async Task<(string message, int retcode, string id)> CreateMemberRole(UInt64 villa_id, string name, string color, Permission permission)
	{
		return await CreateMemberRole(mysBot[mysBot.Count - 1], villa_id, name, color, permission);
	}
	public static async Task<(string message, int retcode, string id)> CreateMemberRole(MysBot mysBot, UInt64 villa_id, string name, string color, Permission permission)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.CreateMemberRole);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 编辑身份组
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="id">身份组ID</param>
	/// <param name="new_name">身份组新名字</param>
	/// <param name="new_color">身份组新颜色</param>
	/// <param name="new_permission">身份组新权限</param>
	/// <returns></returns>
	public static async Task<(string message, int retcode)> EditMemberRole(UInt64 villa_id, UInt64 id, string new_name, string new_color, Permission new_permission)
	{
		return await EditMemberRole(mysBot[mysBot.Count - 1], villa_id, id, new_name, new_color, new_permission);
	}
	public static async Task<(string message, int retcode)> EditMemberRole(MysBot mysBot, UInt64 villa_id, UInt64 id, string new_name, string new_color, Permission new_permission)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.EditMemberRole);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 删除身份组
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="id">身份组ID</param>
	/// <returns>message:返回消息,retcode:返回消息code</returns>
	public static async Task<(string message, int retcode)> DeleteMemberRole(UInt64 villa_id, UInt64 id)
	{
		return await DeleteMemberRole(mysBot[mysBot.Count - 1], villa_id, id);
	}
	public static async Task<(string message, int retcode)> DeleteMemberRole(MysBot mysBot, UInt64 villa_id, UInt64 id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.DeleteMemberRole);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 获取大别野身份组信息
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="role_id">身份组ID</param>
	/// <returns></returns>	
	public static async Task<(string message, int retcode, MemberRole member_role)> GetVillaMemberRoleInfo(UInt64 villa_id, UInt64 role_id)
	{
		return await GetVillaMemberRoleInfo(mysBot[mysBot.Count - 1], villa_id, role_id);
	}
	public static async Task<(string message, int retcode, MemberRole member_role)> GetVillaMemberRoleInfo(MysBot mysBot, UInt64 villa_id, UInt64 role_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetVillaMemberRoleInfo);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 获取大别野所有身份组列表
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <returns>message:返回消息,retcode:返回消息code,member_roles:MemberRole类型列表消息</returns>	
	public static async Task<(string message, int retcode, List<MemberRole> member_roles)> GetVillaMemberRoleList(UInt64 villa_id)
	{
		return await GetVillaMemberRoleList(mysBot[mysBot.Count - 1], villa_id);
	}
	public static async Task<(string message, int retcode, List<MemberRole> member_roles)> GetVillaMemberRoleList(MysBot mysBot, UInt64 villa_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetVillaMemberRole);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));

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

	/// <summary>
	/// 审核消息
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="audit_content">audit_content</param>
	/// <param name="uid">uid</param>
	/// <param name="content_type">消息类型</param>
	/// <param name="pass_through">传递参数</param>
	/// <param name="room_id">房间ID</param>
	/// <returns>message:返回消息,retcode:返回消息code</returns>
	public static async Task<(string message, int retcode)> Audit(UInt64 villa_id, string audit_content, UInt64 uid, Content_Type content_type, string pass_through = "", UInt64 room_id = 0)
	{
		return await Audit(mysBot[mysBot.Count - 1], villa_id, audit_content, uid, content_type, pass_through, room_id);
	}
	public static async Task<(string message, int retcode)> Audit(MysBot mysBot, UInt64 villa_id, string audit_content, UInt64 uid, Content_Type content_type, string pass_through = "", UInt64 room_id = 0)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.Audit);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { audit_content, uid, content_type = Enum.GetName(typeof(Content_Type), content_type), pass_through, room_id });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"审核{res.Content.ReadAsStringAsync().Result}");
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

	#region 图片

	/// <summary>
	/// 将非米游社的三方图床图片转存到米游社官方图床
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="url">需要转存的图片url</param>
	/// <returns>message:返回消息,retcode:返回消息code,new_url:转存后的图片url</returns>
	public static async Task<(string message, int retcode, string new_url)> TransferImage(UInt64 villa_id, string url)
	{
		return await TransferImage(mysBot[mysBot.Count - 1], villa_id, url);
	}
	public static async Task<(string message, int retcode, string new_url)> TransferImage(MysBot mysBot, UInt64 villa_id, string url)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.TransferImage);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
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

	/// <summary>
	/// 上传本地图片至米游社大别野
	/// </summary>
	/// <param name="villa_id">大别野ID</param>
	/// <param name="file_path">需要上传的图片的路径</param>
	/// <returns></returns>
	public static async Task<(string message, int retcode, string url)> UploadImage(UInt64 villa_id, string file_path)
	{
		return await UploadImage(mysBot[mysBot.Count - 1], villa_id, file_path);
	}
	public static async Task<(string message, int retcode, string url)> UploadImage(MysBot mysBot, UInt64 villa_id, string file_path)
	{
		//上传至米游社获取阿里云参数
		string[] allowExt = { "jpg", "jpeg", "png", "gif", "bmp" };
		var md5 = GetMD5Hash.GetMD5HashFromFile(file_path);
		var ext = file_path.Split('.')[file_path.Split('.').Length - 1];
		if (!allowExt.Any(p => p == ext))
		{
			return new() { retcode = -1, message = Logger.LogError("不允许的上传图片扩展名") };
		}

		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.UploadImage);
		httpRequestMessage.AddHeaders(FormatHeader(mysBot, villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { md5, ext });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取米游社阿里云 OSS 上传参数{res.Content.ReadAsStringAsync().Result}");
		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { }
		};
		var json = JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType);

		if (json!.retcode != 0)
		{
			return new() { retcode = json.retcode, message = json.message };
		}

		//上传床图
		var oss_params_json = JObject.Parse(res.Content.ReadAsStringAsync().Result)["data"]!["params"];

		HttpRequestMessage oss_httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, (string?)oss_params_json!["host"]);
		//oss_httpRequestMessage.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data;");

		var fileStream = new FileStream(file_path, FileMode.Open, FileAccess.Read);
		var streamContent = new StreamContent(fileStream);

		var oss_content = new MultipartFormDataContent()
		{
			{ new StringContent((string)oss_params_json["callback_var"]!["x:extra"]!),"\"x:extra\""},
			{ new StringContent((string)oss_params_json["accessid"]!), "\"OSSAccessKeyId\"" },
			{ new StringContent((string)oss_params_json["signature"]!), "\"signature\"" },
			{ new StringContent((string)oss_params_json["success_action_status"]!), "\"success_action_status\"" },
			{ new StringContent((string)oss_params_json["name"]!), "\"name\"" },
			{ new StringContent((string)oss_params_json["callback"]!), "\"callback\"" },
			{ new StringContent((string)oss_params_json["x_oss_content_type"]!), "\"x-oss-content-type\"" },
			{ new StringContent((string)oss_params_json["key"]!), "\"key\"" },
			{ new StringContent((string)oss_params_json["policy"]!), "\"policy\"" },
			{ streamContent,"\"file\"",$"\"Upload.{ext}\""}
		};
		streamContent.Headers.ContentDisposition!.FileNameStar = null;

		var boundary = oss_content.Headers.ContentType!.Parameters.First(o => o.Name == "boundary");
		boundary.Value = boundary.Value!.Replace("\"", String.Empty);

		//修改ContentType与ContentDisposition顺序
		for (int i = 0; i < oss_content.Count(); i++)
		{
			oss_content.ElementAt(i).Headers.ContentType = null;
		}
		oss_httpRequestMessage.Content = oss_content;
		var oss_res = await HttpClass.SendAsync(oss_httpRequestMessage);
		fileStream.Close();

		Logger.Debug($"调用阿里云对象存储 OSS 的 API 上传文件{oss_res.Content.ReadAsStringAsync().Result}");

		var oss_AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { url = "", secret_url = "", @object = "" }
		};
		var oss_json = JsonConvert.DeserializeAnonymousType(oss_res.Content.ReadAsStringAsync().Result, oss_AnonymousType);

		return new() { retcode = oss_json!.retcode, message = oss_json.message, url = oss_json.data.url };
	}

	#endregion

}