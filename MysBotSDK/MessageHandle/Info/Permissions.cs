using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public class Permissions
	{
		public string key { get; set; }
		public string name { get; set; }
		public string describe { get; set; }
	}
	[Flags]
	public enum Permission
	{
		mention_all = 1,
		recall_message = 2,
		pin_message = 4,
		manage_member_role = 8,
		edit_villa_info = 16,
		manage_group_and_room = 32,
		villa_silence = 64,
		black_out = 128,
		handle_apply = 256,
		manage_chat_room = 512,
		view_data_board = 1024,
		manage_custom_event = 2048,
		live_room_order = 4096,
		manage_spotlight_collection = 8192,
	}
}
