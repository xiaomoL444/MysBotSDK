using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example;

[SendMessage("", isBlock = true, priority = 0)]
internal class ReceiverModule : IMysReceiverModule
{
	public bool IsEnable { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

	public Task Execute(MessageReceiverBase @base)
	{
		var senderMessageReceiver = (SendMessageReceiver)@base;
		throw new NotImplementedException();
	}
}
internal class TaskModule : IMysTaskModule
{
	public bool IsEnable { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

	public Task Start()
	{
		throw new NotImplementedException();
	}

	public void Unload()
	{
		throw new NotImplementedException();
	}
}
