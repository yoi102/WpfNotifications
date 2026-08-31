using Notifications.Constants;
using Notifications.Enums;
using Notifications.Sample.Messages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Notifications.Sample
{
    /// <summary>Interaction logic for the sample window.</summary>
    public partial class MainWindow : Window
    {
        private readonly NotificationManager _notificationManager;

        public MainWindow()
        {
            NotificationConstants.DefaultNotificationForeground = new SolidColorBrush(Colors.MistyRose);
            NotificationConstants.DefaultNotificationFontSize = 18;
            NotificationConstants.NotificationWidth = 300;
            NotificationConstants.DefaultNotificationFontWeight = FontWeights.Bold;

            InitializeComponent();
            _notificationManager = new NotificationManager(new NotificationManagerOptions
            {
                Overlay = new NotificationOverlayOptions
                {
                    Position = NotificationPosition.TopRight,
                    MaxItems = 7,
                    AllowRemovingPermanentOnOverflow = false,
                    ReverseOrder = true,
                },
            });
            Closed += (_, _) => _notificationManager.Dispose();
        }

        private NotificationTarget SelectedTarget => isInWindow.IsChecked == true
            ? NotificationTarget.Area(notificationArea.Identifier)
            : NotificationTarget.Overlay();

        private static object RandomCustomMessage()
        {
            switch (Random.Shared.Next(0, 4))
            {
                case 0:
                    return new InformationMessage();
                case 1:
                    return new SuccessMessage();
                case 2:
                    return new WarningMessage();
                case 3:
                    return new ErrorMessage();
                default:
                    throw new NotSupportedException();
            }
        }

        private Task<INotificationHandle> ShowSampleAsync(
            object content,
            bool closeOnClick = false,
            TimeSpan? expirationTime = null)
        {
            return _notificationManager.ShowAsync(new NotificationRequest(content)
            {
                Target = SelectedTarget,
                CloseOnClick = closeOnClick,
                ExpirationTime = expirationTime,
                ShowCloseButton = showCloseButton.IsChecked == true,
                ShowCountdownBar = showCountdownBar.IsChecked == true,
            });
        }

        private async Task ExecuteAsync(Func<Task> operation, string successMessage)
        {
            try
            {
                await operation();
                statusText.Foreground = new SolidColorBrush(Color.FromRgb(20, 108, 67));
                statusText.Text = successMessage;
            }
            catch (Exception exception)
            {
                statusText.Foreground = new SolidColorBrush(Color.FromRgb(180, 35, 24));
                statusText.Text = $"操作失败：{exception.Message}";
            }
        }

        private string CurrentSelectionSummary =>
            $"已显示到{(isInWindow.IsChecked == true ? "窗口区域" : "桌面 Overlay")}；" +
            $"关闭按钮：{(showCloseButton.IsChecked == true ? "显示" : "隐藏")}；" +
            $"倒计时条：{(showCountdownBar.IsChecked == true ? "显示" : "隐藏")}。";

        private async void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            await ExecuteAsync(
                () => _notificationManager.ClearAsync(SelectedTarget),
                "当前目标中的通知已清除。");
        }

        private void comb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (notificationArea != null)
            {
                notificationArea.Position = (NotificationPosition)((ComboBox)sender).SelectedIndex;
            }
        }

        private async void CustomButtonClick(object sender, RoutedEventArgs e)
        {
            await ExecuteAsync(
                async () => await ShowSampleAsync(RandomCustomMessage()),
                CurrentSelectionSummary);
        }

        private async void CustomNotificationButtonClick(object sender, RoutedEventArgs e)
        {
            await ExecuteAsync(
                async () => await ShowSampleAsync(new CustomNotification(), expirationTime: TimeSpan.MaxValue),
                $"{CurrentSelectionSummary} 这是永久通知，不会自动关闭。");
        }

        private async void MessageButtonClick(object sender, RoutedEventArgs e)
        {
            await ExecuteAsync(
                async () => await ShowSampleAsync("普通消息"),
                CurrentSelectionSummary);
        }

        private async void MessageWithTitleButtonClick(object sender, RoutedEventArgs e)
        {
            await ExecuteAsync(
                async () => await ShowSampleAsync(new NotificationContent
                {
                    Title = "操作结果",
                    Message = "这是一条带标题和语义类型的消息。",
                    Type = (NotificationType)Random.Shared.Next(0, 4),
                }),
                CurrentSelectionSummary);
        }

        private async void UserControlMessageButtonClick(object sender, RoutedEventArgs e)
        {
            await ExecuteAsync(
                async () => await ShowSampleAsync(new UserControlMessage()),
                CurrentSelectionSummary);
        }
    }
}
