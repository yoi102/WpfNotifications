# Notifications
WPF toast notifications.

## 演示
![PixPin_2024-12-29_22-39-32](https://github.com/user-attachments/assets/9fb6bb0e-64d7-4877-9a9d-bb801d26b328)

## 安装
```
Install-Package WpfNotifications
```

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

## 默认
WpfNotifications 有2种默认通知样式。
* 你可以用以下代码 使用样式1。
```csharp
            notificationManager.Show("title", NotificationArea);
```
![image](https://github.com/user-attachments/assets/248e5c8c-9c0b-466d-9ec9-cabd5be60e9f)
* 亦可使用如下代码使用样式2。
```csharp
            notificationManager.Show("title", "message", NotificationType.Error, NotificationArea);
```
![image](https://github.com/user-attachments/assets/366f6ccc-029e-45a7-a750-d85015d02a1d)

## 自定义
* 如果默认样式无法满足你的需求，也可传入自定义控件，显示自定义样式

```csharp
            UserControlMessage userControlMessage = new UserControlMessage();
            notificationManager.Show(userControlMessage, "main_window",false);
```

* 在自定义控件中，可继承`Notification`监听`ntf:Notification.NotificationClosing`路由事件，获取`ExpirationTime`值以设置倒计时条.
   *通知控件的宽高可通过`Width``Height`来控制
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
* 在继承`ntf:Notification`后，可设置按钮在Csharp代码中控制通知的关闭

```csharp
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

```





