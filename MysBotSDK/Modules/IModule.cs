using MysBotSDK.MessageHandle;
using System.Threading.Tasks;

namespace MysBotSDK.Modules
{

	/// <summary>
	/// MysBotSDK接口基类
	/// </summary>
	public interface IMysSDKBaseModule
	{
		/// <summary>
		/// 模块是否启用
		/// </summary>
		bool IsEnable { get; set; }
	}
	/// <summary>
	/// 接收器事件接口
	/// </summary>
	public interface IMysReceiverModule : IMysSDKBaseModule
	{
		/// <summary>
		/// 触发器
		/// </summary>
		/// <param name="base">传递的接收器参数</param>
		/// <returns></returns>
		public Task Execute(MessageReceiverBase @base);
	}

	/// <summary>
	/// 多线程启动接口
	/// </summary>
	public interface IMysTaskModule : IMysSDKBaseModule
	{
		/// <summary>
		/// 程序启动时执行的命令
		/// </summary>
		/// <returns></returns>
		public Task Start();

		/// <summary>
		/// 程序卸载插件时执行的命令
		/// </summary>
		public void Unload();
	}
}