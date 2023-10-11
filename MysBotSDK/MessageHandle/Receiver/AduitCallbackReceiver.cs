using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Receiver
{

	public class AuditCallbackReceiver : MessageReceiverBase
	{
		public AuditCallback? auditCallback { get; set; }
		public AuditCallbackReceiver(string message) : base(message)
		{
		}
		public override void Initialize(string message)
		{
			auditCallback = JsonConvert.DeserializeObject<AuditCallback>(message);
		}
	}

}
