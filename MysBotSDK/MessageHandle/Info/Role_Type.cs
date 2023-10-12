using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysBotSDK.MessageHandle.Info
{
	public enum Role_type
	{
		MEMBER_ROLE_TYPE_ALL_MEMBER = 0,//所有人身份组
		MEMBER_ROLE_TYPE_ADMIN = 1,//管理员身份组
		MEMBER_ROLE_TYPE_OWNER = 2,//大别野房主身份组
		MEMBER_ROLE_TYPE_CUSTOM = 3,//其他自定义身份组
		MEMBER_ROLE_TYPE_UNKNOWN = 4//未知
	}
}
