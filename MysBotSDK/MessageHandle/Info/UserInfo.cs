using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info;

/// <summary>
/// 用户信息(SendMessageReceiver用)
/// </summary>
public class UserInfo
{
	/// <summary>
	/// 用户id
	/// </summary>
	public UInt64 id { get; set; }

	/// <summary>
	/// 用户头像(当用户头像为自定义时为空值)
	/// </summary>
	public string? portraitUri { get; set; }

	/// <summary>
	/// ???这个是什么
	/// </summary>
	public string? alias { get; set; }

	/// <summary>
	/// 用户额外信息
	/// </summary>
	[JsonProperty("extra")]
	private string? extra_ { get; set; }

	/// <summary>
	/// 用户额外信息
	/// </summary>
	[JsonIgnore]
	public UserInfo_ExtraInfo extra { get { return JsonConvert.DeserializeObject<UserInfo_ExtraInfo>(extra_!)!; } }
}
/// <summary>
/// 用户信息的extra
/// </summary>

public class UserInfo_ExtraInfo
{
	/// <summary>
	/// 用户此时使用的身份组
	/// </summary>
	public MemberRole? member_roles { get; set; }

	/*
	 public () state;
	public () decoration;
	*/
}
