namespace MysBotSDK.Connection.WebSocket
{

	internal class WebSocketMessage
	{
		/// <summary>
		/// 协议数据。数据采用protobuf Marshal之后的二进制数组
		/// </summary>
		public byte[]? BodyData { get; set; }
		/// <summary>
		/// 用于标识报文的开始 目前的协议的magic值是十六进制的【0xBABEFACE】
		/// </summary>
		public uint Magic = 0xBABEFACE;
		/// <summary>
		/// 变长部分总长度=变长头长度+变长消息体长度
		/// </summary>
		public uint DataLen { get; set; }
		/// <summary>
		/// 变长头总长度，变长头部分所有字段（包括HeaderLen本身）的总长度。
		/// </summary>
		public uint HeaderLen { get; set; }
		/// <summary>
		/// 协议包序列ID，同一条连接上的发出的协议包应该单调递增，相同序列ID且Flag字段相同的包应该被认为是同一个包
		/// </summary>
		public ulong ID { get; set; }
		/// <summary>
		/// 配合bizType使用，用于标识同一个bizType协议的方向。用 1 代表主动发到服务端的request包用 2 代表针对某个request包回应的response包
		/// </summary>
		public uint Flag { get; set; }
		/// <summary>
		/// 消息体的业务类型，用于标识Body字段中的消息所属业务类型
		/// </summary>
		public uint BizType { get; set; }
		/// <summary>
		/// 应用标识。固定为 104
		/// </summary>
		public uint AppId { get; set; }
	}
}