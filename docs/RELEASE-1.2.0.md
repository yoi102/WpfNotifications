# WpfNotifications 1.2.0

日期：2026-09-27（Asia/Tokyo）。改进通知出现与关闭的动效，同时保留现有消息管理、取消、溢出与释放模型。

## 改动

- 动画从默认样式移到 `Notification` / `NotificationMotion` 生命周期，默认与自定义模板都能使用。
- 默认入场 340 ms、退出 260 ms、位移 24 DIP；不再缩放 `LayoutTransform`，避免紧凑窗口反复改变大小和位置。
- 提供入场时长、退出时长、位移距离、延迟入场和手动入场 API。关闭从当前动画帧继续，等待真实完成事件后才移除；宿主卸载结束等待。
- 同步设置隐藏动画时钟，避免等待下一渲染帧才隐藏造成闪现。尊重系统关闭动画设置，支持零时长和 `AnimationsEnabled=false`。
- 原有自定义 Storyboard 需要移除重复动效，或禁用库动效；自定义模板可指定 `PART_AnimationRoot`，无此部件则动画整个控件。

## 本地验证

- `dotnet build Notifications.sln -c Release`：7 个库目标框架与两个示例，0 警告/错误。日志 `artifacts/motion-build.log`。
- `dotnet test Notifications.Tests/Notifications.Tests.csproj -c Release`：net10/net8/net48 各 60/60，共 180 项执行，0 跳过。日志 `artifacts/motion-tests.log`，TRX 在 `artifacts/motion-tests`，最终时间戳 20260927004310/12。
- `eng/SmokeTestPackage.ps1 -PackageDirectory artifacts/packages -PackageVersion 1.2.0`：干净 WPF NuGet 消费者安装、公开 API 编译通过。日志 `artifacts/motion-package-smoke.log`。
- Cadoryx 的 NuGet 消费、Release 发布及桌面验证通过；下游证据 `Cadoryx/artifacts/smoke-20260927-004524`。自定义模板动画、固定尺寸/定位、深浅主题、关闭/取消/退出释放均通过，WPF 绑定错误为零。
- 三处 net10 程序集（源码、干净消费者、Cadoryx）的 SHA256 一致：`FF0484BB2DF62F8C2B34BE707A23DFB9EC46A80FBEF2775E550FDAFF6E102308`。
- 未运行完整多屏混合 DPI、RDP、高对比度和读屏验收；本轮未重复 Debug 矩阵和长期压力测试。参见 `UI-ACCEPTANCE.md`，不把本机结果描述为所有设备上的视觉验收。

## 发布

- 用户已明确授权发布到 nuget.org。主包和配套 `.snupkg` 均已完成 Submit；公开页面已显示安装命令、主包与符号下载链接。00:52 JST，NuGet v3 索引包含 1.2.0，主包下载 HTTP 200。
- 版本页面：https://www.nuget.org/packages/WpfNotifications/1.2.0
- 本地原始 nupkg SHA256：`E0F1A1E781E503050D22F5DC579F058183EB087E90C2F6E1572BCE9BBD9B71DA`。NuGet 仓库签名可能改变包文件哈希，程序集内容应保持一致。
- 源码改动保留在本地工作区，没有创建 GitHub 发布或推送源码。

- Cadoryx 已移除临时本地包源，重新从 nuget.org 还原；缓存元数据 source 为 https://api.nuget.org/v3/index.json。公开包 contentHash 与验证包相同；dotnet nuget verify --all 验证 NuGet.org 仓库签名通过，重新 Release 构建零警告/错误。
