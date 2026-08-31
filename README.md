# WpfNotifications

轻量级、可主题化的 WPF 应用内通知和多显示器桌面 Overlay 通知组件。

![演示](https://github.com/user-attachments/assets/9fb6bb0e-64d7-4877-9a9d-bb801d26b328)

## 安装

```powershell
Install-Package WpfNotifications
```

默认主题会通过 WPF 的主题机制自动加载，不再要求修改 `App.xaml`。旧项目中手动合并的 `Styles/Generic.xaml` 可以继续保留。

## 快速开始

在窗口中声明一个具有唯一标识的通知区域：

```xaml
<ntf:NotificationArea xmlns:ntf="https://github.com/notifications/xaml/controls"
                      Identifier="main_window"
                      MaxItems="3"
                      NotificationMargin="10,10,10,0" />
```

向该区域发送通知：

```csharp
using var manager = new NotificationManager();

manager.Show("普通消息", "main_window");
manager.Show(
    "保存成功",
    "文件已经写入磁盘",
    NotificationType.Success,
    "main_window");
```

省略区域标识符时，会使用不抢焦点的紧凑桌面 Overlay：

```csharp
manager.Show("后台任务已经完成");
```

`Show` 和 `Clear` 可从后台线程调用，UI 操作会自动转发到管理器所属的 Dispatcher。旧 API 保持兼容。

后台旧 API 无法直接返回异常；可以订阅 `manager.Error`。需要可靠地等待结果或捕获异常时，优先使用下面的异步 API。

## 可观察的异步 API

新 API 使用明确的目标，并返回可控制单条通知的句柄。找不到区域或区域标识重复时会直接报告错误，不再静默丢弃消息。

```csharp
var handle = await manager.ShowAsync(new NotificationRequest("正在下载…")
{
    Target = NotificationTarget.Area("main_window"),
    ExpirationTime = TimeSpan.MaxValue,
    ShowCloseButton = true,
    ShowCountdownBar = false,
    Tag = "download",
});

await handle.UpdateAsync("下载完成");
var reason = await handle.Completion;
```

也可以主动关闭单条通知：

```csharp
await handle.CloseAsync();
```

高级 API 默认不会因为点击内容而关闭；如需整条点击关闭，显式设置 `CloseOnClick = true`。

`ShowCloseButton` 和 `ShowCountdownBar` 可以控制单条通知的关闭按钮与倒计时条；保持为 `null` 时使用管理器默认值。隐藏倒计时条不会停止自动关闭。

直接使用 `NotificationArea` 时，可以通过显示选项获得相同行为：

```csharp
area.Show(
    "保存成功",
    TimeSpan.FromSeconds(5),
    new NotificationDisplayOptions
    {
        ShowCloseButton = true,
        ShowCountdownBar = false,
        PauseOnHover = true,
    });
```

`Completion` 会返回 `Programmatic`、`User`、`Expired`、`Cleared`、`Overflow`、`Replaced` 或 `ManagerDisposed`，方便业务层准确处理生命周期。

常见调用可以使用更短的扩展方法：

```csharp
using Notifications.Extensions;

var areaHandle = await manager.ShowAsync("保存成功", "main_window");
var overlayHandle = await manager.ShowOverlayAsync("后台任务完成", NotificationMonitor.MousePointer);
await manager.ClearAsync("main_window");
```

## 去重和更新

带相同 `Tag` 的活动通知可以保留多条、忽略新请求、更新原通知或替换原通知：

```csharp
await manager.ShowAsync(new NotificationRequest("下载 50%")
{
    Target = NotificationTarget.Area("main_window"),
    Tag = "download",
    DuplicateBehavior = NotificationDuplicateBehavior.UpdateExisting,
    ExpirationTime = TimeSpan.MaxValue,
});
```

## 多显示器 Overlay

Overlay 可显示在主屏、鼠标当前所在屏或指定宿主窗口所在屏：

```csharp
await manager.ShowAsync(new NotificationRequest("操作完成")
{
    Target = NotificationTarget.Overlay(NotificationMonitor.MousePointer),
});

await manager.ShowAsync(new NotificationRequest("与窗口关联的消息")
{
    Target = NotificationTarget.Overlay(NotificationMonitor.Owner, this),
});
```

Overlay 只占通知内容区域，使用 `WS_EX_NOACTIVATE`，不会因为透明全屏窗口遮挡其他应用；最后一条通知关闭后窗口会自动释放。

## 实例级配置

每个管理器可以拥有独立设置，不需要修改全局静态状态：

```csharp
var manager = new NotificationManager(new NotificationManagerOptions
{
    DefaultExpirationTime = TimeSpan.FromSeconds(8),
    PauseOnHover = true,
    PauseOnKeyboardFocus = true,
    ShowCloseButton = true,
    ShowCountdownBar = true,
    Overlay = new NotificationOverlayOptions
    {
        MaxItems = 5,
        Position = NotificationPosition.TopRight,
        Margin = new Thickness(12),
        Topmost = true,
    },
});
```

管理器会复制一份配置快照。使用完成后应调用 `Dispose()`；推荐通过 `using` 声明，它会关闭该管理器创建的活动通知并释放 Overlay 窗口：

```csharp
using var manager = new NotificationManager(options);
```

为兼容旧代码，`NotificationConstants` 仍然有效；即使在 `Application` 创建前设置，管理器初始化时也会同步已配置的主题资源。

## 易用性和无障碍

- 默认模板提供可本地化的关闭按钮；应用内通知获得焦点后可按 `Esc` 关闭。
- 鼠标悬停或键盘焦点进入时默认暂停倒计时。
- 通知使用 UI Automation Live Region，便于屏幕阅读器播报。
- 系统关闭客户端动画时，会跳过关闭动画等待。
- 内置状态色已调整为与白色文字满足更清晰的对比度。
- Overlay 默认最多显示 5 条，避免永久通知无限撑满屏幕。

传入 `TimeSpan.MaxValue` 可创建永久通知：

```csharp
manager.Show("需要手动处理", "main_window", expirationTime: TimeSpan.MaxValue);
```

## 自定义内容和主题

可以传入普通对象、任意 `UIElement`，也可以继承 `Notification`。自定义通知可以继续使用 `Close()`、`CloseAsync()`、`ScheduleClose(...)` 和 `ScheduleCloseAsync(...)`。需要响应倒计时开始时，使用语义明确的 `ExpirationScheduled` 路由事件；旧 `NotificationClosing` 事件继续保留兼容性。

应用资源可覆盖默认主题：

```xaml
<SolidColorBrush x:Key="Notifications.StringNotificationBackground"
                 Color="#242424" />
<SolidColorBrush x:Key="Notifications.DefaultNotificationForeground"
                 Color="White" />
<system:Double x:Key="Notifications.DefaultNotificationFontSize"
               xmlns:system="clr-namespace:System;assembly=mscorlib">16</system:Double>
<FontWeight x:Key="Notifications.DefaultNotificationFontWeight">SemiBold</FontWeight>
```

## 目标框架

- 主支持：.NET 10、.NET 8（Windows/WPF）
- 兼容目标：.NET 9、7、6，以及 .NET Framework 4.7、4.8

.NET 6、7 已结束官方支持；新项目建议使用 .NET 10 LTS。CI 会构建全部目标框架，在 .NET 10、.NET 8 和 .NET Framework 4.8 上运行同一套 WPF STA 测试，并从生成的 NuGet 包创建干净 WPF 项目完成安装冒烟测试。

涉及布局、动画或 Overlay 行为的版本，发布前还应执行[真实 UI 环境验收清单](docs/UI-ACCEPTANCE.md)。
