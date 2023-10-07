using MysBotSDK.MessageHandle;

namespace MysBotSDK;

public interface IMysPluginModule
{
	public abstract bool Enable { get; set; }
	public abstract void Execute(MessageReceiver MessageReceiver);
}