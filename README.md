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

实例化后可使用MessageSender类异步发送消息
## 实现的接口
- [ ] 鉴权
  - [ ] 校验用户机器人访问凭证 //有但未测试过，因此未实现完...
- [x] 大别野
  - [x] 获取大别野信息
- [ ] 用户
  - [x] 获取用户信息
  - [x] 获取大别野成员列表
  - [ ] 踢出大别野用户 //咕...没有更多成员可以测了...
- [x] 消息
  - [x] 置顶消息
  - [x] 撤回消息
  - [x] 发送消息
    - [x] 文本
      - [x] @At
        - [x] @At玩家
        - [x] @At所有人
    - [x] 跳转房间
    - [x] 引用链接
  - [x] 发送图片
  - [x] 发送帖子
- [ ] 房间
  - [x] 创建分组
  - [x] 编辑分组
  - [x] 删除分组
  - [x] 获取分组列表
  - [ ] 创建房间 //官方未给出接口
  - [x] 编辑房间
  - [x] 删除房间
  - [x] 获取房间信息
  - [x] 获取房间列表信息
- [x] 身份组
  - [x] 向身份组操作用户
  - [x] 创建身份组
  - [x] 编辑身份组
  - [x] 删除身份组
  - [x] 获取身份组
  - [x] 获取大别野下所有身份组
- [x] 表态表情
  - [x] 获取全量表情
- [ ] 审核
  - [ ] 审核 //有但未测试过，因此未实现完...
- [x] 图片
  - [x] 图片转存

未归类的方法

~~重载方法(放弃)~~