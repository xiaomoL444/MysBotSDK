namespace MysBotSDK.Modules;

#region ExtenData
/// <summary>
/// 接收器特征
/// </summary>
public abstract class ExtendDataAttribute : Attribute { }

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
	/// 唤起的命令
	/// </summary>
	public string Commond { get; set; }
	/// <summary>
	/// 触发该方法的命令，如命令"/Test"，则填入"Test"或者"、Test"，发送消息时可用"@bot /Test"或者"@bot Test"唤起
	/// </summary>
	/// <param name="commond"></param>
	public SendMessageAttribute(string commond)
	{
		if (commond[0] == '/')
		{
			Commond = commond.Substring(1);
		}
		Commond = commond;
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

#region Program

public abstract class ProgramAttribute : Attribute { }

public class StartAttribute : ProgramAttribute { }
#endregion