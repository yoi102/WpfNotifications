# Notifications
WPF toast notifications.

## 演示
![PixPin_2024-12-29_22-39-32](https://github.com/user-attachments/assets/9fb6bb0e-64d7-4877-9a9d-bb801d26b328)


## 使用
* 修改你的App.xaml 文件。*注意：ShutdownMode="OnMainWindowClose" 此设置防止程序无法正常关闭。
```xaml

<Application x:Class="Notifications.Sample.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             ShutdownMode="OnMainWindowClose"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/Notifications;component/Styles/Generic.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>

```

* 在需要显示通知的地方放置`ntf:NotificationArea` 并且设置`Identifier`

```xaml
            <ntf:NotificationArea Identifier="main_window"
                                  NotificationMargin="10 10 10 0"
                                  MaxItems="3"/>

```


* 指定`Identifier`显示通知
```csharp
            notificationManager.Show("title", "main_window");
```
* 也可传入自定义控件，显示自定义UI的通知

```csharp
            UserControlMessage userControlMessage = new UserControlMessage();
            notificationManager.Show(userControlMessage, "main_window",false);
```



* 在自定义控件中，可继承`Notification`监听`ntf:Notification.NotificationClosing`路由事件，获取`ExpirationTime`值以设置倒计时条.
 * 在自定义控件中，可以自定义设置`Width``Height`来控制通知的宽高。
```xaml
<ntf:Notification x:Class="Notifications.Sample.Messages.InformationMessage"
                  ............
                  xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                  xmlns:ntf="https://github.com/notifications/xaml/controls"
                  mc:Ignorable="d"
                  Width="300"
                  Height="100"
                  Name="notification"
                  d:DesignHeight="100"
                  d:DesignWidth="300">
    <ntf:Notification.Triggers>
        <EventTrigger RoutedEvent="ntf:Notification.NotificationClosing">
            <BeginStoryboard>
                <Storyboard>
                    <DoubleAnimation Storyboard.TargetName="colorBar"
                                     Storyboard.TargetProperty="Width"
                                     To="0"
                                     Duration="{Binding ElementName=notification, Path=ExpirationTime}" />
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </ntf:Notification.Triggers>
    ............
</ntf:Notification>

```








