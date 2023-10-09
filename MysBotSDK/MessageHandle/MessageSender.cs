using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MysBotSDK.MessageHandle;

public static class MessageSender
{
	public static string header { get; internal set; } = "";//没有x-rpc-bot_villa_id
	public static string FormatHeader(int villa_id) { return header + $"\nx-rpc-bot_villa_id:{villa_id}"; }
	private static MysBot mysBot;
	internal static MysBot MysBot
	{
		set
		{
			mysBot = value;
			header = @$"x-rpc-bot_id:{mysBot.bot_id}
x-rpc-bot_secret:{Authentication.HmacSHA256(mysBot.secret, mysBot.pub_key)}
Content-Type: application/json";
		}
	}
	#region 消息发送方法
	public static async Task<object> SendText(int villa_id, UInt64 room_id, MessageChain msg_content)
	{
		await msg_content.Bulid();

		MsgContentInfo msgContentInfo = new MsgContentInfo();
		string object_name = "MHY:Text";
		msgContentInfo.content = new MsgContent() { text = msg_content.text_, entities = msg_content.entities_ };

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
		return JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType).data;
	}
	public static async Task<object> SendImage(int villa_id, UInt64 room_id, string url, PicContentInfo.Size size = null, int file_size = 0)
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
		return JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType).data;
	}
	public static async Task<object> SendPost(int villa_id, UInt64 room_id, string post_id)
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
		return JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType).data;
	}
	public static async Task<object> RecallMessage(int villa_id, UInt64 room_id, string msg_uid, Int64 msg_time)
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
		return JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType).data;
	}
	public static async Task<object> PinMessage(int villa_id, UInt64 room_id, string msg_uid, Int64 msg_time, bool is_cancel)
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
		return JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType).data;
	}
	#endregion
	#region API获取信息
	/// <summary>
	/// 获取用户信息
	/// </summary>
	/// <param name="villa_id">大别野id</param>
	/// <param name="user_id">用户id</param>
	/// <returns></returns>
	public static async Task<Member> GetUserInformation(int villa_id, UInt64 user_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetUserInformation);
		httpRequestMessage.AddHeaders(FormatHeader(villa_id));
		httpRequestMessage.Content = JsonContent.Create(new { uid = user_id });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Debug($"获取用户信息{res.Content.ReadAsStringAsync().Result}");
		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { member = new Member() }
		};
		return JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType).data.member;
	}
	public static async Task<Villa> GetVillaInformation(int villa_id)
	{

		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetVillaInformation);
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
		return JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType).data.villa;
	}
	public static async Task<Room> GetRoomInformation(int villa_id, UInt64 room_id)
	{

		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetRoomInformation);
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
		return JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType).data.room;
	}
	public static async Task<List<Member>> GetVillaMember(int villa_id)
	{
		int size_count = 10;
		int result_count = 0;
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
			members.AddRange(json.data.list);
			result_count = json.data.list.Count;
		} while (result_count== size_count);
		return members;
	}

	#endregion
}