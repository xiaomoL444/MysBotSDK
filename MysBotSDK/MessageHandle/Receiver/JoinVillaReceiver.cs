using MysBotSDK.MessageHandle.ExtendData;
using MysBotSDK.Tool;
using Newtonsoft.Json;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace MysBotSDK.MessageHandle.Receiver
{
	/// <summary>
	/// 用户加入大别野事件接收器
	/// </summary>
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
		/// <summary>
		/// 加入大别野事件接收器
		/// </summary>
		/// <param name="message"></param>
		public JoinVillaReceiver(string message) : base(message)
		{
			joinVilla = GetExtendDataMsg<JoinVilla>(message);
			villa_id = joinVilla.villa_id;

			Logger.Debug($"Receive [JoinVilla] @{NickName} Form villa:{villa_id},room:{room_id}");
		}
	}
}
