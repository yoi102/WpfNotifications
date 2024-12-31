using Notifications.Enums;
using Notifications.Extensions;
using Notifications.Mvvm.Sample.Interfaces;
using Notifications.Mvvm.Sample.Messages;

namespace Notifications.Mvvm.Sample.Services
{
    internal class NotificationService : INotificationService
    {
        private readonly INotificationManager notificationManager;

        public NotificationService(INotificationManager notificationManager)
        {
            this.notificationManager = notificationManager;
        }

        public void Clear(string areaIdentifier = "")
        {
            notificationManager.Clear(areaIdentifier);
        }

        public void ShowCustomNotification1(string areaIdentifier = "",
                                             bool closeOnClick = true,
                                             TimeSpan? expirationTime = null,
                                             Action? onClick = null,
                                             Action? onClose = null)
        {
            CustomNotification1 customNotification1 = new();

            notificationManager.Show(customNotification1, areaIdentifier, closeOnClick, expirationTime, onClick, onClose);
        }

        public void ShowCustomNotification2(string areaIdentifier = "",
                                             bool closeOnClick = true,
                                             TimeSpan? expirationTime = null,
                                             Action? onClick = null,
                                             Action? onClose = null)
        {
            CustomNotification2 customNotification2 = new();

            notificationManager.Show(customNotification2, areaIdentifier, closeOnClick, expirationTime, onClick, onClose);
        }

        public void ShowDefaultMessage(string title,
                                    string message,
                                    NotificationType notificationType,
                                    string areaIdentifier = "",
                                    bool closeOnClick = true,
                                    TimeSpan? expirationTime = null,
                                    Action? onClick = null,
                                    Action? onClose = null)
        {
            notificationManager.Show(title,
                                     message,
                                     notificationType,
                                     areaIdentifier,
                                     closeOnClick,
                                     expirationTime,
                                     onClick,
                                     onClose);
        }

        public void ShowDefaultMessage(string message,
                               string areaIdentifier = "",
                               bool closeOnClick = true,
                               TimeSpan? expirationTime = null,
                               Action? onClick = null,
                               Action? onClose = null)
        {
            notificationManager.Show(message,
                                    areaIdentifier,
                                    closeOnClick,
                                    expirationTime,
                                    onClick,
                                    onClose);
        }

        public void ShowUserControlMessage(string string1,
                                                                                   string string2,
                                           string areaIdentifier = "",
                                           bool closeOnClick = true,
                                           TimeSpan? expirationTime = null,
                                           Action? onClick = null,
                                           Action? onClose = null)
        {
            UserControlMessage userControlMessage = new(string1, string2);

            notificationManager.Show(userControlMessage, areaIdentifier, closeOnClick, expirationTime, onClick, onClose);
        }
    }
}