using MysBotSDK.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.WebSocket
{
	internal class WebSocket
	{
		public static async Task GetWebSocketInfo(string bot_id, string bot_secret, uint villa_id = 0)
		{
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, Setting.GetWebSocketInfo);

			httpRequestMessage.AddHeaders(@$"x-rpc-bot_id:{bot_id}
x-rpc-bot_secret:{bot_secret}
x-rpc-bot_villa_id:{villa_id}
x-rpc-bot_ts:{(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMicroseconds}
x-rpc-bot_nonce:{Guid.NewGuid()}
Content-Type:application/json");

			var res = await HttpClass.SendAsync(httpRequestMessage);
		}
	}
}
