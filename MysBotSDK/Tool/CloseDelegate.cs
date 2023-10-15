using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.Tool
{
	static class CloseDelegate
	{
		public delegate bool ControlCtrlDelegate(int CtrlType);
		[DllImport("kernel32.dll")]
		internal static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);
		internal static ControlCtrlDelegate cancelHandler = new ControlCtrlDelegate(HandlerRoutine);

		internal static Action CloseEvent { get; set; }
		public static bool HandlerRoutine(int CtrlType)
		{
			switch (CtrlType)
			{
				case 0:
					Logger.Log("Ctrl+C关闭 "); //Ctrl+C关闭  
					break;
				case 2:
					Logger.Log("按控制台关闭按钮关闭");//按控制台关闭按钮关闭  
					break;
			}
			CloseEvent();
			return false;
		}
	}
}
