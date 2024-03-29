# MysBotSDK

顾名思义,C#语言的米游社大别野BotSDK(.Net 7.0)

（文档稍微有些过旧了...咕咕......）

### 作为一份程序集使用

先在Nuget安装以下包: ```Newtonsoft.Json``` ```WebSocketSharp-netstandard``` ```Google.Protobuf``` ```Microsoft.Data.Sqlite``` ```System.Reactive```

引用命名空间

```
using MysBotSDK;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
```

实例化Bot

```
//开启http回调
MysBot mysBot = new MysBot()
{
	http_callback_Address = "",//回调地址,填写你在开发平台输入的回调地址，或者你的回调地址是经过映射的就填写映射的Ip :http://domain.com || 127.0.0.1:3280
	bot_id = "",//开发平台上显示的机器人ID :bot_******
	secret = "",//开发平台上显示的secret
	pub_key = "",//开发平台上显示的pub_key :-----BEGIN PUBLIC KEY-----******-----END PUBLIC KEY----- 此处原样复制即可，我写了删除\r。。。
	loggerLevel = Logger.LoggerLevel.Log,//Log等级，Error>Warning>Log>Debug>Network,不填写此项默认Log，但是Debug与Network内容会记录到日志(.\log\yyyy-mm-dd.db)里
};

//开启ws连接
MysBot mysBot = new MysBot()
{
	WebsocketConnect = true,//表示开启ws连接
	test_villa_id = 0,//若为未上线机器人，此处要填写调试大别野id
	bot_id = "",//开发平台上显示的机器人ID :bot_******
	secret = "",//开发平台上显示的secret
	pub_key = "",//开发平台上显示的pub_key :-----BEGIN PUBLIC KEY-----******-----END PUBLIC KEY----- 此处原样复制即可，我写了删除\r。。。
	loggerLevel = Logger.LoggerLevel.Log,//Log等级，Error>Warning>Log>Debug,不填写此项默认Log，但是Debug与Network内容会记录到日志(.\log\yyyy-mm-dd.db)里
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

### 作为启动程序使用

待...机...中(文档咕咕中...)

#### 模块撰写

同样的

先在Nuget安装以下包: ```Newtonsoft.Json``` ```WebSocketSharp-netstandard``` ```Google.Protobuf``` ```Microsoft.Data.Sqlite``` ```System.Reactive```

引用命名空间

```
using MysBotSDK;
using MysBotSDK.MessageHandle;
using MysBotSDK.MessageHandle.Receiver;
using MysBotSDK.Tool;
```

[示例代码]()

有两个接口，```IMysReceiverModule``` 与 ```IMysTaskModule```

```IMysReceiverModule``` 消息接收模块，为模块添加特征以确认模块用途，目前七种[接收器](https://github.com/xiaomoL444/MysBotSDK/wiki/%E6%8E%A5%E6%94%B6%E5%99%A8)对应七种特征，模块内调用函数将传入接收器的基类，需要通过显示转换将其转换成对应的接收器，如示例代码所示

```IMysTaskModule``` 任务模块，用于程序启动时与关闭时处理的任务(比如定时器之类的)，切记在Stop函数中终止所有线程与计时器任务，否则只是卸载方法会失败

#### 模块调用

启动MysBotSDK.exe，会生成Plugins文件与account.json文件，依据 ### 作为一份程序集使用 内填写account.json ，写好的模块生成后将dll文件放入Plugins文件夹中，程序会主动搜索Plugins下的dll文件，可在程序中输入help查看主动加载或者卸载插件的命令

#### 便捷调试模块文件

将模块文件的输出类型调成控制台应用类型，在Main函数中调用 ```await MysBotSDK.Program.Main(new string[] { "namespace name" });``` namespace name 填写含有模块类的命名空间，(要注意Nuget包要下载全，并且此状态下的控制台加载命名将对调试的模块不起效(卸载还是可以使用的))

### 注意!
- MessageSender中每一个方法各有一个重载，例如```SendText(UInt64 villa_id, UInt64 room_id, MessageChain msg_content)```的一个重载为```SendText(MysBot mysBot, UInt64 villa_id, UInt64 room_id, MessageChain msg_content)```。在多Bot实例化的情况下，使用第一个方法会让最后被实例化的Bot发送消息，使用第二个方法则为指定Bot发送消息。若订阅消息类型为```SendMessageReceiver```时,传回的receiver含有与MessageSender相同的发送方法，使用receiver里的方法发送消息时不再需要填入```villa_id```与```room_id```，且发送的Bot为接收消息的Bot
- http连接内置鉴权，ws连接不支持鉴权

## 实现的接口
(需要villa_id进行鉴权，所以大多接口都需要villa_id)
- [x] 鉴权
  - [x] 校验用户机器人访问凭证
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
- [x] 图片
  - [x] 图片转存 TransferImage(UInt64 villa_id, string url) 
  - [x] 图片上传 UploadImage(UInt64 villa_id, string file_path)

## Other

[MysBotSDK.Tool](https://github.com/xiaomoL444/MysBotSDK/wiki/Tool/)

### [来自官方的提醒]
 
图片转存目前还在灰度阶段，全局QPS限流30，请开发者视需求调用，将会在未来迭代中优化。
如果图片大小过大，或者原图床访问受限（比如海外服务器），可能会导致转存失败。

### Fix

v1.6新增ws连接与按钮消息

v1.5新增按钮面板与相应的回调消息

v1.5新增更多文字样式

v1.5支持在文字里插入图片

v1.4将图片转存Transferimage方法改名为TransferImage,将与以往插件不兼容

## TO DO

~~将计划为SDK添加启动程序~~(好诶，已经完成惹)