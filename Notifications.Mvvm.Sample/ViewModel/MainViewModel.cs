using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Notifications.Enums;
using Notifications.Mvvm.Sample.Interfaces;

namespace Notifications.Mvvm.Sample.ViewModel
{
    internal partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool inWindow = true;

        [ObservableProperty]
        private NotificationPosition notificationPosition = NotificationPosition.BottomRight;

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

        private readonly INotificationService notificationService;

        public MainViewModel(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        [RelayCommand]
        private void CustomNotification1()
        {
            notificationService.ShowCustomNotification1(NotificationArea, false, TimeSpan.MaxValue);
        }

        [RelayCommand]
        private void CustomNotification2()
        {
            notificationService.ShowCustomNotification2(NotificationArea);
        }

        [RelayCommand]
        private void UserControlMessage()
        {
            notificationService.ShowUserControlMessage("xxx_Item", "1", NotificationArea);
        }

        [RelayCommand]
        private void DefaultMessageWithTitle()
        {
            Random random = new Random();
            var type = (Enums.NotificationType)random.Next(0, 4);
            notificationService.ShowDefaultMessage("title", "message", type, NotificationArea);
        }

        [RelayCommand]
        private void DefaultMessage()
        {
            notificationService.ShowDefaultMessage("Message", NotificationArea);
        }

        [RelayCommand]
        private void Clear()
        {
            notificationService.Clear(NotificationArea);
        }
    }
}