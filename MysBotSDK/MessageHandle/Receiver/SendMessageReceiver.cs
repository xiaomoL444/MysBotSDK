using MysBotSDK.MessageHandle.ExtendData;
using MysBotSDK.MessageHandle.Info;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{
	/// <summary>
	/// 有用户发送@At消息事件接收器
	/// </summary>
	public class SendMessageReceiver : MessageReceiverBase
	{
		/// <summary>
		/// 指令，不管传入的命令是否有/，这里一并添加/
		/// </summary>
		public string commond { get; set; }
		/// <summary>
		/// 用户提交的指令参数(不包括开头的@At与/指令，只包括指令后的参数，以空格分离)
		/// </summary>
		public List<string> args => sendMessage.args;
		/// <summary>
		/// 反序列化后的content消息，非必要不使用，里面似乎还有类没实现...（好的，v1.6.2实现了userInfo）
		/// </summary>
		public Content_Msg content => sendMessage.content;
		/// <summary>
		/// 消息文本
		/// </summary>
		public string? Text => sendMessage.content.content!.text;
		/// <summary>
		/// 用户的UID
		/// </summary>
		public UInt64 UID => sendMessage.from_user_id;
		/// <summary>
		/// 消息的发送时间
		/// </summary>
		public Int64 Send_Time => sendMessage.send_at;
		/// <summary>
		/// 房间ID
		/// </summary>
		public UInt64 Room_ID => sendMessage.room_id;
		/// <summary>
		/// 用户昵称
		/// </summary>
		public string? Name => sendMessage.nickname;
		/// <summary>
		/// 消息ID
		/// </summary>
		public string? Msg_ID => sendMessage.msg_uid;
		/// <summary>
		/// 如果被回复的消息从属于机器人，则该字段不为空字符串
		/// </summary>
		public string? Bot_Msg_ID => sendMessage.bot_msg_id;
		/// <summary>
		/// 大别野ID
		/// </summary>
		public UInt64 Villa_ID => sendMessage.villa_id;
		/// <summary>
		/// 消息摘要，如果是文本消息，则返回消息的文本内容。如果是图片消息，则返回"[图片]"
		/// </summary>
		public string? Quote_Content => sendMessage.quote_msg!.content;
		/// <summary>
		/// 消息ID
		/// </summary>
		public string? Quote_Msg_ID => sendMessage.quote_msg!.msg_uid;
		/// <summary>
		/// 如果消息从属于机器人，则该字段不为空字符串
		/// </summary>
		public string? Quote_Bot_ID => sendMessage.quote_msg!.bot_msg_id;
		/// <summary>
		/// 发送时间的时间戳 
		/// </summary>
		public Int64 Quote_Send_Time => sendMessage.quote_msg!.send_at;
		/// <summary>
		/// 消息类型，包括"文本"，"图片"，"帖子卡片"等
		/// </summary>
		public Quote_Msg.Msg_Type Quote_Msg_Type => sendMessage.quote_msg!.msg_type;
		/// <summary>
		/// 发送者的UID
		/// </summary>
		public UInt64 Quote_UID => sendMessage.quote_msg!.from_user_id;
		/// <summary>
		/// 发送者昵称
		/// </summary>
		public string? Quote_Name => sendMessage.quote_msg!.from_user_nickname;
		/// <summary>
		/// 发送者 id（字符串）可携带机器人发送者的id
		/// </summary>
		public string? User_ID_Str => sendMessage.quote_msg!.from_user_id_str;

		/// <summary>
		/// 消息中的图片url数组，支持图文消息、图片消息、自定义表情、avatar互动等消息类型
		/// </summary>
		public List<string> Images => sendMessage.quote_msg!.images;

		internal SendMessage sendMessage { get; set; }
		internal Quote_Msg quote_msg => sendMessage.quote_msg!;
		/// <summary>
		/// 发送At消息接收器
		/// </summary>
		/// <param name="message"></param>
		public SendMessageReceiver(string message) : base(message)
		{
			sendMessage = GetExtendDataMsg<SendMessage>(message);
			villa_id = sendMessage.villa_id;
			room_id = sendMessage.room_id;

			var args = sendMessage!.content.content!.text!.Split(" ").ToList();
			commond = args[1];
			if (!commond.StartsWith("/"))
			{
				commond = "/" + commond;
			}
			args.RemoveRange(0, 2);
			sendMessage.args = args;

			Logger.Log($"Receive [SendMessage] {Text} Form villa:{villa_id},room:{room_id}");
		}

		#region Method

		/// <summary>
		/// 发送一条文本消息
		/// </summary>
		/// <param name="msg_content">消息链(MessageChain)</param>
		/// <returns>message:返回消息,retcode:返回消息code,bot_msg_id:消息uid</returns>
		public async Task<(string message, int retcode, string bot_msg_id)> SendText(MessageChain msg_content)
		{
			return await MessageSender.SendText(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, room_id, msg_content);
		}
		/// <summary>
		/// 发送一条自定义Json消息
		/// </summary>
		/// <param name="msg_content">自定义Json消息</param>
		/// <returns>message:返回消息,retcode:返回消息code,bot_msg_id:消息uid</returns>
		public async Task<(string message, int retcode, string bot_msg_id)> SendText(string msg_content)
		{
			return await MessageSender.SendText(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, room_id, msg_content);
		}

		/// <summary>
		/// 发送图片信息
		/// </summary>
		/// <param name="url">图片url</param>
		/// <param name="size">图片尺寸</param>
		/// <param name="file_size">图片大小</param>
		/// <returns>message:返回消息,retcode:返回消息code,bot_msg_id:消息uid</returns>
		public async Task<(string message, int retcode, string bot_msg_id)> SendImage(string url, PicContentInfo.Size size = null!, int file_size = 0)
		{
			return await MessageSender.SendImage(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, room_id, url, size, file_size);
		}

		/// <summary>
		/// 发送帖子消息
		/// </summary>
		/// <param name="post_id">帖子ID</param>
		/// <returns>message:返回消息,retcode:返回消息code,bot_msg_id:消息uid</returns>
		public async Task<(string message, int retcode, string bot_msg_id)> SendPost(string post_id)
		{
			return await MessageSender.SendPost(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, room_id, post_id);
		}

		/// <summary>
		/// 撤回消息
		/// </summary>
		/// <param name="msg_uid">消息uid</param>
		/// <param name="msg_time">消息发送时间</param>
		/// <returns>message:返回消息,retcode:返回消息code,bot_msg_id:消息uid</returns>
		public async Task<(string message, int retcode, string bot_msg_id)> RecallMessage(string msg_uid, Int64 msg_time)
		{
			return await MessageSender.RecallMessage(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, room_id, msg_uid, msg_time);
		}

		/// <summary>
		/// 置顶消息
		/// </summary>
		/// <param name="msg_uid">消息uid</param>
		/// <param name="msg_time">消息发送时间</param>
		/// <param name="is_cancel">是否取消置顶消息</param>
		/// <returns>message:返回消息,retcode:返回消息code,bot_msg_id:消息uid</returns>
		public async Task<(string message, int retcode, string bot_msg_id)> PinMessage(string msg_uid, Int64 msg_time, bool is_cancel)
		{
			return await MessageSender.PinMessage(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, room_id, msg_uid, msg_time, is_cancel);
		}

		/// <summary>
		/// 创建大别野分组
		/// </summary>
		/// <param name="group_name">组别名字</param>
		/// <returns>message:返回消息,retcode:返回消息code,group_id:组别id</returns>
		public async Task<(string message, int retcode, string group_id)> CreateGroup(string group_name)
		{
			return await MessageSender.CreateGroup(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, group_name);
		}

		/// <summary>
		/// 删除分组
		/// </summary>
		/// <param name="group_id">组别ID</param>
		/// <returns>message:返回消息,retcode:返回消息code</returns>
		public async Task<(string message, int retcode)> DeleteGroup(UInt64 group_id)
		{
			return await MessageSender.DeleteGroup(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, group_id);
		}

		/// <summary>
		/// 编辑组别
		/// </summary>
		/// <param name="group_id">组别ID</param>
		/// <param name="new_group_name">组别新名称</param>
		/// <returns>message:返回消息,retcode:返回消息code</returns>
		public async Task<(string message, int retcode)> EditGroup(UInt64 group_id, string new_group_name)
		{
			return await MessageSender.EditGroup(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, group_id, new_group_name);
		}

		/// <summary>
		/// 编辑房间
		/// </summary>
		/// <param name="new_room_name">房间新名称</param>
		/// <returns>message:返回消息,retcode:返回消息code</returns>
		public async Task<(string message, int retcode)> EditRoom(string new_room_name)
		{
			return await MessageSender.EditRoom(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, room_id, new_room_name);
		}

		/// <summary>
		/// 删除房间
		/// </summary>
		/// <param name="room_id">房间ID</param>
		/// <returns>message:返回消息,retcode:返回消息code</returns>
		public async Task<(string message, int retcode)> DeleteRoom(UInt64 room_id)
		{
			return await MessageSender.DeleteRoom(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, room_id);
		}

		/// <summary>
		/// 获取用户信息
		/// </summary>
		/// <param name="uid">用户UID</param>
		/// <returns>message:返回消息,retcode:返回消息code,member:Member类消息</returns>	
		public async Task<(string message, int retcode, Member member)> GetUserInfo(UInt64 uid)
		{
			return await MessageSender.GetUserInfo(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, uid);
		}

		/// <summary>
		/// 获得大别野信息
		/// </summary>
		/// <returns>message:返回消息,retcode:返回消息code,villa:Villa类消息</returns>	
		public async Task<(string message, int retcode, Villa villa)> GetVillaInfo()
		{
			return await MessageSender.GetVillaInfo(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id);
		}

		/// <summary>
		/// 获取房间信息
		/// </summary>
		/// <param name="room_id">房间ID</param>
		/// <returns>message:返回消息,retcode:返回消息code,room:Room类消息</returns>
		public async Task<(string message, int retcode, Room room)> GetRoomInfo(UInt64 room_id)
		{
			return await MessageSender.GetRoomInfo(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, room_id);
		}

		/// <summary>
		/// 获取大别野所有用户
		/// </summary>
		/// <returns>message:返回消息,retcode:返回消息code,members:Member类列表消息</returns>
		public async Task<(string message, int retcode, List<Member> members)> GetVillaMember()
		{
			return await MessageSender.GetVillaMember(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id);
		}

		/// <summary>
		/// 获取所有组别
		/// </summary>
		/// <returns>message:返回消息,retcode:返回消息code,groups:组别列表消息</returns>
		public async Task<(string message, int retcode, List<Group> groups)> GetGroupList()
		{
			return await MessageSender.GetGroupList(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id);
		}

		/// <summary>
		/// 获取所有房间
		/// </summary>
		/// <returns>message:返回消息,retcode:返回消息code,rooms:Room类型列表消息</returns>	
		public async Task<(string message, int retcode, List<Room> rooms)> GetRoomList()
		{
			return await MessageSender.GetRoomList(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id);
		}

		/// <summary>
		/// 校验用户机器人访问凭证
		/// </summary>
		/// <param name="token">token</param>
		/// <returns>message:返回消息,retcode:返回消息code,access_info:BotMemberAccessInfo类型消息,member:Member类型消息</returns>
		public async Task<(string message, int retcode, BotMemberAccessInfo access_info, Member member)> CheckMemberBotAccessToken(string token)
		{
			return await MessageSender.CheckMemberBotAccessToken(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, token);
		}

		/// <summary>
		/// 获得所有表情
		/// </summary>
		/// <returns>message:返回消息,retcode:返回消息code,emoticons:Emoticon类型列表消息</returns>
		public async Task<(string message, int retcode, List<Emoticon> emoticons)> GetAllEmoticons()
		{
			return await MessageSender.GetAllEmoticons(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id);
		}

		/// <summary>
		/// 提出用户
		/// </summary>
		/// <param name="user_id">用户uid</param>
		/// <returns></returns>
		public async Task<(string message, int retcode)> DeleteVillaMember(UInt64 user_id)
		{
			return await MessageSender.DeleteVillaMember(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, user_id);
		}

		/// <summary>
		/// 更改用户身份组信息
		/// </summary>
		/// <param name="user_id">房间ID</param>
		/// <param name="role_id">身份组ID</param>
		/// <param name="is_add">是否是添加身份组</param>
		/// <returns>message:返回消息,retcode:返回消息code</returns>
		public async Task<(string message, int retcode)> OperateMemberToRole(UInt64 user_id, UInt64 role_id, bool is_add)
		{
			return await MessageSender.OperateMemberToRole(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, user_id, role_id, is_add);
		}

		/// <summary>
		/// 创建身份组
		/// </summary>
		/// <param name="name">身份组名字</param>
		/// <param name="color">身份组颜色</param>
		/// <param name="permission">身份组权限(可通过+=添加权限)</param>
		/// <returns>message:返回消息,retcode:返回消息code,id:身份组ID</returns>	
		public async Task<(string message, int retcode, string id)> CreateMemberRole(string name, string color, Permission permission)
		{
			return await MessageSender.CreateMemberRole(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, name, color, permission);
		}

		/// <summary>
		/// 编辑身份组
		/// </summary>
		/// <param name="id">身份组ID</param>
		/// <param name="new_name">身份组新名字</param>
		/// <param name="new_color">身份组新颜色</param>
		/// <param name="new_permission">身份组新权限</param>
		/// <returns></returns>
		public async Task<(string message, int retcode)> EditMemberRole(UInt64 id, string new_name, string new_color, Permission new_permission)
		{
			return await MessageSender.EditMemberRole(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, id, new_name, new_color, new_permission);
		}

		/// <summary>
		/// 删除身份组
		/// </summary>
		/// <param name="id">身份组ID</param>
		/// <returns>message:返回消息,retcode:返回消息code</returns>
		public async Task<(string message, int retcode)> DeleteMemberRole(UInt64 id)
		{
			return await MessageSender.DeleteMemberRole(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, id);
		}

		/// <summary>
		/// 获取大别野身份组信息
		/// </summary>
		/// <param name="role_id">身份组ID</param>
		/// <returns></returns>	
		public async Task<(string message, int retcode, MemberRole member_role)> GetVillaMemberRoleInfo(UInt64 role_id)
		{
			return await MessageSender.GetVillaMemberRoleInfo(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, role_id);
		}

		/// <summary>
		/// 获取大别野所有身份组列表
		/// </summary>
		/// <returns>message:返回消息,retcode:返回消息code,member_roles:MemberRole类型列表消息</returns>	
		public async Task<(string message, int retcode, List<MemberRole> member_roles)> GetVillaMemberRoleList()
		{
			return await MessageSender.GetVillaMemberRoleList(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id);
		}

		/// <summary>
		/// 审核消息
		/// </summary>
		/// <param name="audit_content">audit_content</param>
		/// <param name="uid">uid</param>
		/// <param name="content_type">消息类型</param>
		/// <param name="pass_through">传递参数</param>
		/// <param name="room_id">房间ID</param>
		/// <returns>message:返回消息,retcode:返回消息code</returns>
		public async Task<(string message, int retcode)> Audit(string audit_content, UInt64 uid, Content_Type content_type, string pass_through = "", UInt64 room_id = 0)
		{
			return await MessageSender.Audit(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, audit_content, uid, content_type, pass_through, room_id);
		}

		/// <summary>
		/// 图片转存
		/// </summary>
		/// <param name="url">需要转存的图片url</param>
		/// <returns>message:返回消息,retcode:返回消息code,new_url:转存后的图片url</returns>
		public async Task<(string message, int retcode, string new_url)> Transferimage(string url)
		{
			return await MessageSender.TransferImage(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, url);
		}

		/// <summary>
		/// 上传本地图片至米游社大别野
		/// </summary>
		/// <param name="file_path">需要上传的图片的路径</param>
		/// <returns></returns>
		public async Task<(string message, int retcode, string url)> UploadImage(string file_path)
		{
			return await MessageSender.UploadImage(MessageSender.mysBot.FirstOrDefault(b => b.bot_id == robot!.template!.id)!, villa_id, file_path);
		}
		#endregion
	}
}
