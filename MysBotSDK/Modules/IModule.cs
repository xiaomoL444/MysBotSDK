using MysBotSDK.MessageHandle;

namespace MysBotSDK.Modules;

/// <summary>
/// 接收器事件接口
/// </summary>
public interface IMysReceiverModule
{
	/// <summary>
	/// 触发器
	/// </summary>
	/// <param name="base">传递的接收器参数</param>
	/// <returns></returns>
	public Task Execute(MessageReceiverBase @base);

	/// <summary>
	/// 是否启用插件
	/// </summary>
	public bool? isEnable { get; set; }
}

/// <summary>
/// 多线程启动接口
/// </summary>
public interface IMysTaskModule
{
	/// <summary>
	/// 程序启动时执行的命令
	/// </summary>
	/// <returns></returns>
	public Task Start();

	/// <summary>
	/// 程序卸载插件时执行的命令
	/// </summary>
	public void UnLoad();

	/// <summary>
	/// 是否启用插件
	/// </summary>
	public bool? isEnable { get; set; }
}