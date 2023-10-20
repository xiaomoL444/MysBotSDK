using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace MysBotSDK.MessageHandle.Info;

public class Entity
{
	/// <summary>
	/// 具体的实体信息
	/// </summary>
	public Entity_Detail entity { get; set; }

	/// <summary>
	/// 表示UTF-16编码下对应实体的长度
	/// </summary>
	public ulong length { get; set; }

	/// <summary>
	/// 表示UTF-16编码下对应实体在 text 中的起始位置
	/// </summary>
	public ulong offset { get; set; }
}
