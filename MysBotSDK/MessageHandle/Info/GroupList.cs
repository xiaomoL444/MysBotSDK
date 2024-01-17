using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info;

public class GroupList : Group
{
	public List<Group> room_list { get; set; } = new();
}
