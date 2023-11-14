# MysBotSDK

顾名思义,C#语言的米游社大别野BotSDK(.Net 7.0)

## 使用

先在Nuget安装以下包: Newtonsoft.Json WebSocketSharp-netstandard

引用命名空间

```
using MysBotSDK;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
```

实例化Bot

```
MysBot mysBot = new MysBot()
{
	ws_callback_Address = ""//ws反代回调地址(不可同时填入ws_callback_Address与http_callback_Address)
	http_callback_Address = "",//回调地址,填写你在开发平台输入的回调地址，或者你的回调地址是经过映射的就填写映射的Ip :http://domain.com || 127.0.0.1:3280
	bot_id = "",//开发平台上显示的机器人ID :bot_******
	secret = "",//开发平台上显示的secret
	pub_key = "",//开发平台上显示的pub_key :-----BEGIN PUBLIC KEY-----******-----END PUBLIC KEY----- 此处原样复制即可，我写了删除\r。。。
	loggerLevel = Logger.LoggerLevel.Log,//Log等级，Error>Warning>Log>Debug,不填写此项默认Log，但是Debug内容会记录到日志(.\log\yyyy-mm-dd.txt)里
};
```

MysBot实现了IDisposable接口，可通过mysBot.Dispose()释放mysBot

实例化后,MysBot有一个MessageReceiver属性可以订阅事件,这里订阅一个"用户@AtBot消息"(```SendMessageReceiver```)(所有可以订阅的消息类型可以看[接收器](https://github.com/xiaomoL444/MysBotSDK/wiki/%E6%8E%A5%E6%94%B6%E5%99%A8))

```
mysBot.MessageReceiver
	.OfType<SendMessageReceiver>()
	.Subscribe((receiver) =>
	{
    /*
      YourCode
    */
	});
```

实例化后可使用```MessageSender```类异步发送消息,例如发送文本,只有发送文本这里需要构造消息链(别的为什么不用?...?)

构造消息链,[wiki](https://github.com/xiaomoL444/MysBotSDK/wiki/MessageChain)列举了所有可以用于构造的方法

```
var messageChain = new MessageChain()
.Text("")
.At(receiver.Villa_ID, 0)
.AtAll()
.Url_Link("")
.Quote(receiver.Msg_ID, receiver.Send_Time)
.Room_Link(receiver.Villa_ID, receiver.Room_ID);
```

发送文本

```
MessageSender.SendText(receiver.Villa_ID,receiver.Room_ID,messageChain);
```

MessageSender更多用法请看[#实现的接口](#实现的接口)

### 注意!
- MessageSender中每一个方法各有一个重载，例如```SendText(UInt64 villa_id, UInt64 room_id, MessageChain msg_content)```的一个重载为```SendText(MysBot mysBot, UInt64 villa_id, UInt64 room_id, MessageChain msg_content)```。在多Bot实例化的情况下，使用第一个方法会让最后被实例化的Bot发送消息，使用第二个方法则为指定Bot发送消息。若订阅消息类型为```SendMessageReceiver```时,传回的receiver含有与MessageSender相同的发送方法，使用receiver里的方法发送消息时不再需要填入```villa_id```与```room_id```，且发送的Bot为接收消息的Bot
- http连接内置鉴权，ws连接不支持鉴权

## 实现的接口
(需要villa_id进行鉴权，所以大多接口都需要villa_id)
- [ ] 鉴权
  - [ ] 校验用户机器人访问凭证 //有但未测试过
- [x] 大别野
  - [x] 获取大别野信息 GetVillaInfo(UInt64 Villa_ID)
- [ ] 用户
  - [x] 获取用户信息 GetUserInfo(UInt64 Villa_ID,UInt64 UID)
  - [x] 获取大别野成员列表 GetVillaMember(UInt64 Villa_ID)
  - [ ] 踢出大别野用户 //咕...没有更多成员可以测了...
- [x] 消息
  - [x] 置顶消息 PinMessage(UInt64 villa_id, UInt64 room_id, string msg_uid, Int64 msg_time, bool is_cancel)
  - [x] 撤回消息 RecallMessage(UInt64 villa_id, UInt64 room_id, string msg_uid, Int64 msg_time)
  - [x] 发送消息 SendText(UInt64 villa_id, UInt64 room_id, MessageChain msg_content)
    - [x] 文本
      - [x] @At 
        - [x] @At玩家 At(UInt64 villa_id, UInt64 id)
        - [x] @At所有人 AtAll()
    - [x] 跳转房间 Room_Link(UInt64 villa_id, UInt64 room_id)
    - [x] 引用链接 Url_Link(string url, bool requires_bot_access_token = false)
  - [x] 发送图片 SendImage(UInt64 villa_id, UInt64 room_id, string url, PicContentInfo.Size size = null, int file_size = 0)
  - [x] 发送帖子 SendPost(UInt64 villa_id, UInt64 room_id, string post_id)
- [x] 房间
  - [x] 创建分组 CreateGroup(UInt64 villa_id, string group_name)
  - [x] 编辑分组 EditGroup(UInt64 villa_id, UInt64 group_id, string new_group_name)
  - [x] 删除分组 DeleteGroup(UInt64 villa_id, UInt64 group_id)
  - [x] 获取分组列表 GetGroupList(UInt64 villa_id)
  - [ ] 创建房间 //官方已删
  - [x] 编辑房间 EditRoom(UInt64 villa_id, UInt64 room_id, string new_room_name)
  - [x] 删除房间 DeleteRoom(UInt64 villa_id, UInt64 room_id)
  - [x] 获取房间信息 GetRoomInfo(UInt64 villa_id, UInt64 room_id)
  - [x] 获取房间列表信息 GetRoomList(UInt64 villa_id)
- [x] 身份组
  - [x] 向身份组操作用户 EditMemberRole(UInt64 villa_id, UInt64 id, string new_name, string new_color, Permission new_permission)
  - [x] 创建身份组 CreateMemberRole(UInt64 villa_id, string name, string color, Permission permission)
  - [x] 编辑身份组 EditMemberRole(UInt64 villa_id, UInt64 id, string new_name, string new_color, Permission new_permission)
  - [x] 删除身份组 DeleteMemberRole(UInt64 villa_id, UInt64 id)
  - [x] 获取身份组 GetVillaMemberRoleInfo(UInt64 villa_id, UInt64 role_id)
  - [x] 获取大别野下所有身份组 GetVillaMemberRoleList(UInt64 villa_id)
- [x] 表态表情
  - [x] 获取全量表情 GetAllEmoticons(UInt64 villa_id)
- [ ] 审核
  - [ ] 审核 Audit(UInt64 villa_id, string audit_content, UInt64 uid, Content_Type content_type, string pass_through = "", UInt64 room_id = 0) //有但未测试过
- [ ] 图片
  - [x] 图片转存 Transferimage(UInt64 villa_id, string url) 
  - [ ] 图片上传

## Other

[MysBotSDK.Tool](https://github.com/xiaomoL444/MysBotSDK/wiki/Tool/)

### ?
 
图片转存目前还在灰度阶段，全局QPS限流30，请开发者视需求调用，将会在未来迭代中优化。
如果图片大小过大，或者原图床访问受限（比如海外服务器），可能会导致转存失败。

### Fix

## TO DO

如果学到了更好的语句与方法就尝试重构一些方法

有一些报错似乎也没有弄好...(要多实现几个类吗...?)(不是不知道哪里有bug，是一些类似网络断开等导致运行中断的这种，可能还需要大家自己写try...?要是写在SDK里面还是会throw错误)

写了个ws重新连接，但是可能连接超时没有再次重新连接...?

# MihoyoBBS_Bot
嗯...一个个人的启动程序...?好像不用理会，有用的只有MysBotSDK