# MysBotSDK
## 使用
引用命名空间
```
using MysBotSDK;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
using System.Reactive.Linq;
```
实例化Bot
```
MysBot mysBot = new MysBot()
{
	callback_Adress = "",//回调地址,填写你在开发平台输入的回调地址，或者你的回调地址是经过映射的就填写映射的Ip :http://domain.com || 127.0.0.1:3280
	bot_id = "",//开发平台上显示的机器人ID :bot_******
	secret = "",//开发平台上显示的secret
	pub_key = "",//开发平台上显示的pub_key :-----BEGIN PUBLIC KEY-----******-----END PUBLIC KEY----- 此处原样复制即可，我写了删除\r。。。
	loggerLevel = Logger.LoggerLevel.Log,//Log等级，Error>Warning>Log>Debug,不填写默认Log，但是Debug内容会记录到日志(.\log\yyyy-mm-dd.txt)里
};
```
MysBot未实现IDisposable接口
实例化后,MysBot有一个MessageReceiver属性可以订阅事件,这里订阅一个用户@AtBot消息(所有可以订阅的消息类型可以看这里[接收器](https://github.com/xiaomoL444/MysBotSDK/wiki/%E6%8E%A5%E6%94%B6%E5%99%A8))
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

实例化后可使用MessageSender类异步发送消息,例如发送文本,只有发送文本这里需要构造消息链(别的为什么不用?...?)
构造消息链,这里列举了所有可以用于构造的方法,Text("")插入一段文本,At(villa_id,uid)@At指定用户,AtAll()@At所有人 !!!注意At与AtAll不能同时使用 ,Url_Link("")插入一段url,Quote(message_id,send_time)引用消息,Room_Link(villa_id,room_id)插入一段#跳转房间,
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
MessageSender更多用法请看[#实现的接口](https://github.com/xiaomoL444/MysBotSDK#%E5%AE%9E%E7%8E%B0%E7%9A%84%E6%8E%A5%E5%8F%A3)

## 实现的接口
(需要villa_id进行鉴权，所以大多接口都需要villa_id)
- [ ] 鉴权
  - [ ] 校验用户机器人访问凭证 //有但未测试过，因此未实现完...
- [x] 大别野
  - [x] 获取大别野信息 GetVillaInfo(UInt64 Villa_ID)
- [ ] 用户
  - [x] 获取用户信息 GetUserInfo(UInt64 Villa_ID,UInt64 UID)
  - [x] 获取大别野成员列表 GetVillaMember(UInt64 Villa_ID)
  - [ ] 踢出大别野用户 根本没实现...//咕...没有更多成员可以测了...
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
- [ ] 房间
  - [x] 创建分组 CreateGroup(UInt64 villa_id, string group_name)
  - [x] 编辑分组 EditGroup(UInt64 villa_id, UInt64 group_id, string new_group_name)
  - [x] 删除分组 DeleteGroup(UInt64 villa_id, UInt64 group_id)
  - [x] 获取分组列表 GetGroupList(UInt64 villa_id)
  - [ ] 创建房间 //官方未给出接口
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
  - [ ] 审核 Audit(UInt64 villa_id, string audit_content, UInt64 uid, Content_Type content_type, string pass_through = "", UInt64 room_id = 0) //有但未测试过，因此未实现完...
- [x] 图片
  - [x] 图片转存 Transferimage(UInt64 villa_id, string url)

未归类的方法

~~重载方法(放弃)~~