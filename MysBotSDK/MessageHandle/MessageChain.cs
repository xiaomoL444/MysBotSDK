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
		internal string text { get; set; }
		internal List<Entity> entities { get; set; }
		public MessageChain()
		{
			text = string.Empty.ConvertUTF8ToUTF16();
			entities = new List<Entity>();
		}
		public MessageChain Text(string text)
		{
			this.text += text.ConvertUTF8ToUTF16();
			return this;
		}
		public async Task<MessageChain> At(int villa_id, UInt64 id)
		{
			var member = await MessageSender.GetUserInformation(villa_id, id);
			entities.Add(new Entity()
			{
				entity = new Entity.entity_detail() { type = Entity.entity_detail.EntityType.mentioned_user, user_id = id.ToString() },
				length = (ulong)$"@{member.basic.nickname.ConvertUTF8ToUTF16()}".Length,
				offset = (ulong)text.Length
			});
			Text($"@{member.basic.nickname}");
			return this;
		}
	}
}
