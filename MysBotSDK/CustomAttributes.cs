namespace MysBotSDK;

public abstract class ExtendDataAttribute : Attribute
{

}
public class JoinVillaAttribute : ExtendDataAttribute
{

}
public class SendMessageAttribute : ExtendDataAttribute
{
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
public class CreateRobotAttribute : ExtendDataAttribute
{

}
public class DeleteRobotAttribute : ExtendDataAttribute
{

}
public class AddQuickEmoticonAttribute : ExtendDataAttribute
{

}
public class AuditCallbackAttribute : ExtendDataAttribute
{

}
