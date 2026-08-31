using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Notifications.Mvvm.Sample.Interfaces;

namespace Notifications.Mvvm.Sample.ViewModel
{
    internal partial class MainViewModel : ObservableObject
    {
        private readonly INotificationService notificationService;

        [ObservableProperty]
        private bool inWindow = true;

        [ObservableProperty]
        private bool showCloseButton = true;

        [ObservableProperty]
        private bool showCountdownBar = true;

        [ObservableProperty]
        private string statusMessage = "请选择选项并创建通知；显示开关仅影响之后创建的通知。";

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private bool hasCompletedOperation;

        public MainViewModel(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        public string NotificationArea
        {
            get
            {
                if (InWindow == true)
                {
                    return "main_window";
                }
                return "";
            }
        }
        [RelayCommand]
        private Task Clear()
        {
            return ExecuteAsync(
                () => notificationService.ClearAsync(NotificationArea),
                "当前目标中的通知已清除。");
        }

        [RelayCommand]
        private Task CustomNotification1()
        {
            return ExecuteAsync(
                () => notificationService.ShowCustomNotification1Async(
                    NotificationArea,
                    closeOnClick: false,
                    expirationTime: TimeSpan.MaxValue,
                    showCloseButton: ShowCloseButton,
                    showCountdownBar: ShowCountdownBar),
                $"{CurrentSelectionSummary} 这是永久通知，不会自动关闭。");
        }

        [RelayCommand]
        private Task CustomNotification2()
        {
            return ExecuteAsync(
                () => notificationService.ShowCustomNotification2Async(
                    NotificationArea,
                    showCloseButton: ShowCloseButton,
                    showCountdownBar: ShowCountdownBar),
                CurrentSelectionSummary);
        }

        [RelayCommand]
        private Task DefaultMessage()
        {
            return ExecuteAsync(
                () => notificationService.ShowDefaultMessageAsync(
                    "普通消息",
                    NotificationArea,
                    showCloseButton: ShowCloseButton,
                    showCountdownBar: ShowCountdownBar),
                CurrentSelectionSummary);
        }

        [RelayCommand]
        private Task DefaultMessageWithTitle()
        {
            var type = (Enums.NotificationType)Random.Shared.Next(0, 4);
            return ExecuteAsync(
                () => notificationService.ShowDefaultMessageAsync(
                    "操作结果",
                    "这是一条带标题和语义类型的消息。",
                    type,
                    NotificationArea,
                    showCloseButton: ShowCloseButton,
                    showCountdownBar: ShowCountdownBar),
                CurrentSelectionSummary);
        }

        [RelayCommand]
        private Task UserControlMessage()
        {
            return ExecuteAsync(
                () => notificationService.ShowUserControlMessageAsync(
                    "示例项目",
                    "1",
                    NotificationArea,
                    showCloseButton: ShowCloseButton,
                    showCountdownBar: ShowCountdownBar),
                CurrentSelectionSummary);
        }

        private string CurrentSelectionSummary =>
            $"已显示到{(InWindow ? "窗口区域" : "桌面 Overlay")}；" +
            $"关闭按钮：{(ShowCloseButton ? "显示" : "隐藏")}；" +
            $"倒计时条：{(ShowCountdownBar ? "显示" : "隐藏")}。";

        private async Task ExecuteAsync(Func<Task> operation, string successMessage)
        {
            try
            {
                await operation();
                HasError = false;
                HasCompletedOperation = true;
                StatusMessage = successMessage;
            }
            catch (Exception exception)
            {
                HasError = true;
                HasCompletedOperation = true;
                StatusMessage = $"操作失败：{exception.Message}";
            }
        }
    }
}
