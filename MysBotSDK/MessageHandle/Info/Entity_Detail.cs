using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace MysBotSDK.MessageHandle.Info
{
	public class Entity_Detail
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public EntityType type { get; set; }
		public enum EntityType
		{
			mentioned_robot = 0,
			mentioned_user = 1,
			mentioned_all = 2,
			villa_room_link = 3,
			link = 4
		}
		public string bot_id { get; set; }
		public string user_id { get; set; }
		public string villa_id { get; set; }
		public string room_id { get; set; }
		public string url { get; set; }
		public bool requires_bot_access_token { get; set; }
	}
}
