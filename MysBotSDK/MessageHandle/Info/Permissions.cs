using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class Permissions
	{
		/// <summary>
		/// 权限 key 字符串
		/// </summary>
		public string? key { get; set; }

		/// <summary>
		/// 权限名称
		/// </summary>
		public string? name { get; set; }

		/// <summary>
		/// 权限描述
		/// </summary>
		public string? describe { get; set; }
	}
	[Flags]
	public enum Permission
	{
		mention_all = 1,//允许成员能够 @全体成员
		recall_message = 2,//允许成员能够在聊天房间中撤回任何人的消息
		pin_message = 4,//允许成员能够在聊天房间中置顶消息
		manage_member_role = 8,//允许成员添加、删除身份组，管理身份组成员，修改身份组的权限
		edit_villa_info = 16,//允许成员编辑大别野的简介、标签、设置大别野加入条件等
		manage_group_and_room = 32,//允许成员新建房间，新建/删除房间分组，调整房间及房间分组的排序
		villa_silence = 64,//允许成员能够在房间里禁言其他人
		black_out = 128,//允许成员能够拉黑和将其他人移出大别野
		handle_apply = 256,//允许成员审核大别野的加入申请
		manage_chat_room = 512,//允许成员编辑房间信息及设置可见、发言权限
		view_data_board = 1024,//允许成员查看大别野数据看板
		manage_custom_event = 2048,//允许成员创建活动，编辑活动信息
		live_room_order = 4096,//允许成员在直播房间中点播节目及控制节目播放
		manage_spotlight_collection = 8192,//允许成员设置、移除精选消息
	}
}
