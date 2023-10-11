using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{
	public class JoinVillaReceiver : MessageReceiverBase
	{
		/// <summary>
		/// 用户UID
		/// </summary>
		public UInt64 UID => joinVilla.join_uid;
		/// <summary>
		/// 用户昵称
		/// </summary>
		public string NickName => joinVilla.join_user_nickname;
		/// <summary>
		/// 用户加入时间
		/// </summary>
		public Int64 JoinTime => joinVilla.join_at;
		/// <summary>
		/// 大别野ID
		/// </summary>
		public UInt64 Villa_ID => joinVilla.villa_id;
		internal JoinVilla joinVilla { get; set; }
		public JoinVillaReceiver(string message) : base(message)
		{

		}
		public override void Initialize(string message)
		{
			joinVilla = JsonConvert.DeserializeObject<JoinVilla>(message)!;
		}
	}
}
