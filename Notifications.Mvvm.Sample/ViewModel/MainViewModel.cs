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
        private void Clear()
        {
            notificationService.Clear(NotificationArea);
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
        private void DefaultMessage()
        {
            notificationService.ShowDefaultMessage("Message", NotificationArea);
        }

        [RelayCommand]
        private void DefaultMessageWithTitle()
        {
            Random random = new Random();
            var type = (Enums.NotificationType)random.Next(0, 4);
            notificationService.ShowDefaultMessage("title", "message", type, NotificationArea);
        }

        [RelayCommand]
        private void UserControlMessage()
        {
            notificationService.ShowUserControlMessage("xxx_Item", "1", NotificationArea);
        }
    }
}