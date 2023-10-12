using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace MysBotSDK.MessageHandle.Info;

public class Entity
{
	public Entity_Detail entity { get; set; }
	public ulong length { get; set; }
	public ulong offset { get; set; }
}
