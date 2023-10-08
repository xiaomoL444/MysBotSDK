﻿using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MysBotSDK.MessageHandle;

public static class MessageSender
{
	public static string header { get; internal set; } = "";//没有x-rpc-bot_villa_id
	public static string FormatHeader(string villa_id) { return header += $"\n{villa_id}"; }
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
	public static async Task<object> SendText(int villa_id, ulong room_id, MessageChain msg_content)
	{
		await msg_content.Bulid();

		MsgContentInfo msgContentInfo = new MsgContentInfo();
		string object_name = "MHY:Text";
		msgContentInfo.content = new MsgContent() { text = msg_content.text_, entities = msg_content.entities_ };

		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, Setting.SendMessage);
		httpRequestMessage.AddHeaders(FormatHeader($"{villa_id}"));

		httpRequestMessage.Content = JsonContent.Create(new { room_id, object_name, msg_content = JsonConvert.SerializeObject(msgContentInfo) });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Log("Back" + res.Content.ReadAsStringAsync().Result);
		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { bot_msg_id = "" }
		};
		return JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType).data;
	}
	public static async void SendImage(int villa_id, ulong room_id)
	{
		MsgContentInfo msgContentInfo = new MsgContentInfo();
		string object_name = "MHY:Image";
	}
	public static async void SendPost(int villa_id, ulong room_id)
	{
		MsgContentInfo msgContentInfo = new MsgContentInfo();
		string object_name = "MHY:Post";
	}
	/// <summary>
	/// 获取用户信息
	/// </summary>
	/// <param name="villa_id">大别野id</param>
	/// <param name="user_id">用户id</param>
	/// <returns></returns>
	public static async Task<Member> GetUserInformation(int villa_id, UInt64 user_id)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetUserInformation);
		httpRequestMessage.AddHeaders(FormatHeader($"{villa_id}"));
		httpRequestMessage.Content = JsonContent.Create(new { uid = user_id });
		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Log($"获取用户信息{res.Content.ReadAsStringAsync().Result}");
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
		httpRequestMessage.AddHeaders(FormatHeader($"{villa_id}"));
		httpRequestMessage.Content = JsonContent.Create(new { villa_id = villa_id });

		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Log($"获取大别野信息{res.Content.ReadAsStringAsync().Result}");
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
		httpRequestMessage.AddHeaders(FormatHeader($"{villa_id}"));
		httpRequestMessage.Content = JsonContent.Create(new { room_id = room_id });

		var res = await HttpClass.SendAsync(httpRequestMessage);
		Logger.Log($"获取房间信息{res.Content.ReadAsStringAsync().Result}");
		var AnonymousType = new
		{
			retcode = 0,
			message = "",
			data = new { room = new Room() }
		};
		return JsonConvert.DeserializeAnonymousType(res.Content.ReadAsStringAsync().Result, AnonymousType).data.room;
	}
}