namespace MysBotSDK.Modules;

#region ExtenData
/// <summary>
/// 接收器特征
/// </summary>
public abstract class ExtendDataAttribute : Attribute
{
	/// <summary>
	/// 优先级，328优先级比0大，仅针对同一接收器有效
	/// </summary>
	public int priority { get; set; }
}

/// <summary>
/// 有用户加入大别野的接收器特征
/// </summary>
public class JoinVillaAttribute : ExtendDataAttribute { }

/// <summary>
/// 有用户发送@at消息的接收器特征
/// </summary>
public class SendMessageAttribute : ExtendDataAttribute
{
	/// <summary>
	/// 查找到该方法后是否锁定停止往下搜索方法
	/// </summary>
	public bool isBlock { get; set; }

	/// <summary>
	/// 唤起的命令
	/// </summary>
	public string command { get; set; }
	/// <summary>
	/// 触发该方法的命令，如命令"/Test"，则填入"Test"或者"、Test"，发送消息时可用"@bot /Test"或者"@bot Test"唤起
	/// </summary>
	/// <param name="commond"></param>
	public SendMessageAttribute(string command)
	{
		if (command[0] == '/')
		{
			command = command.Substring(1);
		}
		command = command;
	}
}

/// <summary>
/// Bot加入大别野的接收器特征
/// </summary>
public class CreateRobotAttribute : ExtendDataAttribute { }

/// <summary>
/// Bot被移出大别野的接收器特征
/// </summary>
public class DeleteRobotAttribute : ExtendDataAttribute { }

/// <summary>
/// 用户发送表情回复消息接收器特征
/// </summary>
public class AddQuickEmoticonAttribute : ExtendDataAttribute { }

/// <summary>
/// 审核事件的接收器特征
/// </summary>
public class AuditCallbackAttribute : ExtendDataAttribute { }

/// <summary>
/// 面板点击事件的接收器特征
/// </summary>
public class ClickMsgComponentAttribute : ExtendDataAttribute { }
#endregion