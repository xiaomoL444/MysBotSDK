using MysBotSDK.MessageHandle.Info;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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
		internal string text_ { get; set; }
		internal List<Entity> entities_ { get; set; }
		internal MentionType mentionType { get; set; }
		internal QuoteInfo? quote { get; set; }
		private List<string> text { get; set; }
		private List<(int index, Entity entity)> IDs { get; set; }

		public UInt64 template_id { get; set; }
		public List<List<Component_Group>> smallComponent { get; private set; } = new List<List<Component_Group>>();
		public List<List<Component_Group>> midComponent { get; private set; } = new List<List<Component_Group>>();
		public List<List<Component_Group>> bigComponent { get; private set; } = new List<List<Component_Group>>();

		public List<PicContentInfo> images { get; set; } = new();

		public MessageChain()
		{
			text_ = string.Empty;
			entities_ = new List<Entity>();
			text = new List<string>();
			Text("");
			IDs = new List<(int, Entity)>();
			mentionType = MentionType.None;
		}

		/// <summary>
		/// 插入一段文本
		/// </summary>
		/// <param name="text">文本内容</param>
		/// <returns>消息链</returns>
		public MessageChain Text(string text)
		{
			this.text.Add(text.ConvertUTF8ToUTF16());
			return this;
		}

		/// <summary>
		/// 插入一段@At消息(不能与AtAll()同时使用)
		/// </summary>
		/// <param name="villa_id">大别野ID</param>
		/// <param name="id">@At用户UID</param>
		/// <returns>消息链</returns>
		public MessageChain At(UInt64 villa_id, UInt64 id)
		{
			IDs.Add((text.Count - 1, new Entity()
			{
				entity = new Entity_Detail()
				{
					type = Entity_Detail.EntityType.mentioned_user,
					villa_id = villa_id.ToString(),
					user_id = id.ToString()
				}
			}));
			return this;
		}

		/// <summary>
		/// @At所有人(不能与At()同时使用)
		/// </summary>
		/// <returns>消息链</returns>
		public MessageChain AtAll()
		{
			IDs.Add((text.Count - 1, new Entity()
			{
				entity = new Entity_Detail()
				{
					type = Entity_Detail.EntityType.mentioned_all
				}
			}));
			return this;
		}

		/// <summary>
		/// 引用一段消息
		/// </summary>
		/// <param name="message_id">消息UID</param>
		/// <param name="message_send_time">消息发送时间</param>
		/// <returns>消息链</returns>
		public MessageChain Quote(string message_id, Int64 message_send_time)
		{
			quote = new QuoteInfo();
			quote.quoted_message_id = message_id;
			quote.quoted_message_send_time = message_send_time;
			quote.original_message_id = message_id;
			quote.original_message_send_time = message_send_time;
			return this;
		}

		/// <summary>
		/// 插入一段跳转房间#room
		/// </summary>
		/// <param name="villa_id">大别野ID</param>
		/// <param name="room_id">房间ID</param>
		/// <returns></returns>
		public MessageChain Room_Link(UInt64 villa_id, UInt64 room_id)
		{
			IDs.Add((text.Count - 1, new Entity()
			{
				entity = new Entity_Detail()
				{
					type = Entity_Detail.EntityType.villa_room_link,
					villa_id = villa_id.ToString(),
					room_id = room_id.ToString()
				}
			}));
			return this;
		}

		/// <summary>
		/// 跳转外部链接
		/// </summary>
		/// <param name="url">url链接</param>
		/// <param name="requires_bot_access_token">是否需要带上含有用户信息的token</param>
		/// <returns></returns>
		public MessageChain Url_Link(string url, bool requires_bot_access_token = false)
		{
			IDs.Add((text.Count - 1, new Entity()
			{
				entity = new Entity_Detail()
				{
					type = Entity_Detail.EntityType.link,
					url = url,
					requires_bot_access_token = requires_bot_access_token
				}
			}));
			return this;
		}

		public MessageChain AddButtonComponent(Component_Size component_Size, List<Component_Group> component_Group)
		{
			switch (component_Size)
			{
				case Component_Size.small:
					smallComponent.Add(component_Group);
					break;
				case Component_Size.middle:
					midComponent.Add(component_Group);
					break;
				case Component_Size.big:
					bigComponent.Add(component_Group);
					break;
				default:
					break;
			}

			return this;
		}
		public MessageChain Image(string url, PicContentInfo.Size size = null!, int file_size = 0)
		{
			images.Add(new PicContentInfo() { url = url, size = size, file_size = file_size });
			return this;
		}

		internal async Task<MessageChain> Bulid()
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
						case Entity_Detail.EntityType.mentioned_robot:
							break;
						case Entity_Detail.EntityType.mentioned_user:
							mentionType = MentionType.Partof;
							var member = await MessageSender.GetUserInfo(UInt64.Parse(entity.entity!.entity.villa_id!), UInt64.Parse(entity.entity.entity!.user_id!));
							entities_.Add(new Entity()
							{
								entity = new Entity_Detail() { type = Entity_Detail.EntityType.mentioned_user, user_id = entity.entity.entity.user_id },
								length = (ulong)$"@{member.member.basic!.nickname!.ConvertUTF8ToUTF16()} ".Length,
								offset = (ulong)text_.Length
							});

							text_ += $"@{member.member.basic!.nickname!.ConvertUTF8ToUTF16()} ";
							break;
						case Entity_Detail.EntityType.mentioned_all:
							mentionType = MentionType.All;
							entities_.Add(new Entity()
							{
								entity = entity.entity.entity,
								length = (ulong)"@全体成员 ".Length,
								offset = (ulong)text_.Length
							});
							text_ += "@全体成员 ".ConvertUTF8ToUTF16();
							break;
						case Entity_Detail.EntityType.villa_room_link:
							var room = await MessageSender.GetRoomInfo(UInt64.Parse(entity.entity!.entity.villa_id!), UInt64.Parse(entity.entity!.entity.room_id!));
							entities_.Add(new Entity()
							{
								entity = new Entity_Detail() { type = Entity_Detail.EntityType.mentioned_user, user_id = entity.entity.entity.user_id },
								length = (ulong)$"#{room.room!.room_name!.ConvertUTF8ToUTF16()} ".Length,
								offset = (ulong)text_.Length
							});
							text_ += $"#{room.room!.room_name!.ConvertUTF8ToUTF16()} ";
							break;
						case Entity_Detail.EntityType.link:
							entities_.Add(new Entity()
							{
								entity = entity.entity.entity,
								length = (ulong)entity.entity!.entity.url!.Length,
								offset = (ulong)text_.Length
							});
							text_ += entity.entity.entity.url.ConvertUTF8ToUTF16();
							break;
						default:
							break;
					}
				}

				//添加组件

			}
			return this;
		}
	}

	public enum Component_Size
	{
		small = 0,
		middle = 1,
		big = 2,
	}
}
