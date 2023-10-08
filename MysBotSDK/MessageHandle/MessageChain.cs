using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle
{
	internal static class ExtensionMethod
	{
		internal static string ConvertUTF8ToUTF16(this string text)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			return Encoding.Unicode.GetString(Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bytes));
		}
	}
	public class MessageChain
	{
		[JsonProperty("text")]
		internal string text_ { get; set; }
		[JsonProperty("entites")]
		internal List<Entity> entities_ { get; set; }

		private List<string> text { get; set; }
		private List<(int index, Entity entity)> IDs { get; set; }

		public MessageChain()
		{
			text_ = string.Empty;
			entities_ = new List<Entity>();
			text = new List<string>();
			Text("");
			IDs = new List<(int, Entity)>();
		}
		public MessageChain Text(string text)
		{
			this.text.Add(text.ConvertUTF8ToUTF16());
			return this;
		}
		public MessageChain At(int villa_id, UInt64 id)
		{
			IDs.Add((text.Count - 1, new Entity()
			{
				entity = new Entity.entity_detail()
				{
					type = Entity.entity_detail.EntityType.mentioned_user,
					villa_id = villa_id.ToString(),
					user_id = id.ToString()
				}
			}));
			return this;
		}
		public MessageChain AtAll()
		{
			IDs.Add((text.Count - 1, new Entity()
			{
				entity = new Entity.entity_detail()
				{
					type = Entity.entity_detail.EntityType.mentioned_all
				}
			}));
			return this;
		}
		public MessageChain Room_Link(int villa_id, UInt64 room_id)
		{
			IDs.Add((text.Count - 1, new Entity()
			{
				entity = new Entity.entity_detail()
				{
					type = Entity.entity_detail.EntityType.villa_room_link,
					villa_id = villa_id.ToString(),
					room_id = room_id.ToString()
				}
			}));
			return this;
		}
		public async Task<MessageChain> Bulid()
		{
			for (int i = 0; i < text.Count; i++)
			{
				text_ += text[i].ConvertUTF8ToUTF16();
				//添加entites
				var entities = IDs.Where(q => q.index == i);//获取某个index下需要entity，switch判断
				foreach (var entity in entities)
				{
					switch (entity.entity.entity.type)
					{
						case Entity.entity_detail.EntityType.mentioned_robot:
							break;
						case Entity.entity_detail.EntityType.mentioned_user:
							var member = await MessageSender.GetUserInformation(int.Parse(entity.entity.entity.villa_id), UInt64.Parse(entity.entity.entity.user_id));
							entities_.Add(new Entity()
							{
								entity = new Entity.entity_detail() { type = Entity.entity_detail.EntityType.mentioned_user, user_id = entity.entity.entity.user_id },
								length = (ulong)$"@{member.basic.nickname.ConvertUTF8ToUTF16()} ".Length,
								offset = (ulong)text_.Length
							});
							text_ += $"@{member.basic.nickname.ConvertUTF8ToUTF16()} ";
							break;
						case Entity.entity_detail.EntityType.mentioned_all:
							entities_.Add(new Entity()
							{
								entity = entity.entity.entity,
								length = (ulong)"@全体成员 ".Length,
								offset = (ulong)text_.Length
							});
							text_ += "@全体成员 ".ConvertUTF8ToUTF16();
							break;
						case Entity.entity_detail.EntityType.villa_room_link:
							var room = await MessageSender.GetRoomInformation(int.Parse(entity.entity.entity.villa_id), UInt64.Parse(entity.entity.entity.room_id));
							entities_.Add(new Entity()
							{
								entity = new Entity.entity_detail() { type = Entity.entity_detail.EntityType.mentioned_user, user_id = entity.entity.entity.user_id },
								length = (ulong)$"#{room.room_name.ConvertUTF8ToUTF16()} ".Length,
								offset = (ulong)text_.Length
							});
							text_ += $"#{room.room_name.ConvertUTF8ToUTF16()} ";
							break;
						case Entity.entity_detail.EntityType.link:
							break;
						default:
							break;
					}
				}

			}
			return this;
		}
	}
}
