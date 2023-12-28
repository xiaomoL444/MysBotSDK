using MysBotSDK.MessageHandle.Info;
using MysBotSDK.MessageHandle.Receiver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static MysBotSDK.MessageHandle.Info.Entity_Detail;

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

	/// <summary>
	/// 构造消息链
	/// </summary>
	public class MessageChain
	{
		internal string text_ { get; set; }
		internal List<Entity> entities_ { get; set; }
		internal MentionType mentionType { get; set; }
		internal QuoteInfo? quote { get; set; }
		private List<string> text { get; set; }
		private List<(int index, Entity entity)> IDs { get; set; }

		internal UInt64 template_id { get; set; }
		internal List<List<Component_Group>> smallComponent { get; private set; } = new List<List<Component_Group>>();
		internal List<List<Component_Group>> midComponent { get; private set; } = new List<List<Component_Group>>();
		internal List<List<Component_Group>> bigComponent { get; private set; } = new List<List<Component_Group>>();

		internal List<PicContentInfo> images { get; set; } = new();

		/// <summary>
		/// 消息链构造器
		/// </summary>
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
		/// <param name="font_Sytle">字体样式</param>
		/// <returns>消息链</returns>
		public MessageChain Text(string text, Entity_Detail.Font_Sytle font_Sytle = Entity_Detail.Font_Sytle.None)
		{
			if (font_Sytle != Entity_Detail.Font_Sytle.None)
			{
				for (int i = 0; i < Enum.GetNames(typeof(Entity_Detail.Font_Sytle)).Length - 1; i++)
				{
					if (((int)font_Sytle & (1 << i)) != 0)
					{
						this.IDs.Add((this.text.Count - 1, new Entity()
						{
							entity = new Entity_Detail()
							{
								type = Entity_Detail.EntityType.style,
								font_style = (Entity_Detail.Font_Sytle)(1 << i)
							}
						}));
					}
				}
			}
			Text(text);
			return this;
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
		/// 自定义文本样式
		/// </summary>
		/// <param name="offset">文本开始位置</param>
		/// <param name="length">文本长度</param>
		/// <param name="font_Sytle">文本样式</param>
		/// <returns></returns>
		public MessageChain Font_Style(UInt64 offset, UInt64 length, Font_Sytle font_Sytle = Font_Sytle.None)
		{
			if (font_Sytle != Entity_Detail.Font_Sytle.None)
			{
				for (int i = 0; i < Enum.GetNames(typeof(Entity_Detail.Font_Sytle)).Length - 1; i++)
				{
					if (((int)font_Sytle & (1 << i)) != 0)
					{
						entities_.Add(new()
						{
							offset = offset,
							length = length,
							entity = new()
							{
								type = Entity_Detail.EntityType.style,
								font_style = (Entity_Detail.Font_Sytle)(1 << i)
							}
						});
					}
				}

			}

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
		/// 引用一段消息
		/// </summary>
		/// <param name="sendMessageReceiver">sendMessage接收器</param>
		/// <returns>消息链</returns>
		public MessageChain Quote(SendMessageReceiver sendMessageReceiver)
		{
			quote = new QuoteInfo();
			quote.quoted_message_id = sendMessageReceiver.Msg_ID;
			quote.quoted_message_send_time = sendMessageReceiver.Send_Time;
			quote.original_message_id = sendMessageReceiver.Msg_ID;
			quote.original_message_send_time = sendMessageReceiver.Send_Time;
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
		/// <param name="highlight_text">高亮文本</param>
		/// <param name="requires_bot_access_token">是否需要带上含有用户信息的token</param>
		/// <returns></returns>
		public MessageChain Url_Link(string url, bool requires_bot_access_token = false, string highlight_text = "")
		{
			IDs.Add((this.text.Count - 1, new Entity()
			{
				entity = new Entity_Detail()
				{
					type = Entity_Detail.EntityType.link,
					url = url,
					url_highlight_text = highlight_text,
					requires_bot_access_token = requires_bot_access_token
				}
			}));
			return this;
		}

		/// <summary>
		/// 增加按钮组件
		/// </summary>
		/// <param name="component_Size">按钮组件的大小</param>
		/// <param name="component_Group">按钮组件列表(即一行放置的组件)</param>
		/// <returns></returns>
		public MessageChain ButtonComponent(Component_Size component_Size, List<Component_Group> component_Group)
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

		/// <summary>
		/// 文本消息内插入图片
		/// </summary>
		/// <param name="url">图片链接</param>
		/// <param name="size">图片尺寸</param>
		/// <param name="file_size">图片储存空间大小</param>
		/// <returns></returns>
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
							var add_url_text = string.IsNullOrEmpty(entity.entity.entity.url_highlight_text) ? entity.entity.entity.url!.ConvertUTF8ToUTF16() : entity.entity.entity.url_highlight_text.ConvertUTF8ToUTF16();
							entities_.Add(new Entity()
							{
								entity = entity.entity.entity,
								length = (ulong)add_url_text.Length,
								offset = (ulong)text_.Length
							});
							text_ += add_url_text;
							break;
						case Entity_Detail.EntityType.style:
							entities_.Add(new()
							{
								entity = entity.entity.entity,
								length = entity.entity.length == 0 ? (ulong)(text[entity.index + 1].ConvertUTF8ToUTF16().Length) : entity.entity.length,
								offset = entity.entity.offset == 0 ? (ulong)text_.Length : entity.entity.offset
							});
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

	/// <summary>
	/// 面板组件大小
	/// </summary>
	public enum Component_Size
	{
		/// <summary>
		/// 小型组件，即一行摆置3个组件，每个组件最多展示2个中文字符或4个英文字符
		/// </summary>
		small = 0,
		/// <summary>
		/// 中型组件，即一行摆置2个组件，每个组件最多展示4个中文字符或8个英文字符
		/// </summary>
		middle = 1,
		/// <summary>
		/// 大型组件，即一行摆置1个组件，每个组件最多展示10个中文字符或20个英文字符
		/// </summary>
		big = 2,
	}
}
