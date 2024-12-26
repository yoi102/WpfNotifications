using Notifications.Enums;

namespace Notifications.Extensions
{
    public static class NotificationManagerExtension
    {
        public static void Show(this INotificationManager notificationManager,
                                    string title,
                                    string message,
                                    NotificationType notificationType,
                                    string areaIdentifier = "",
                                    bool closeOnClick = true,
                                    TimeSpan? expirationTime = null,
                                    Action? onClick = null,
                                    Action? onClose = null)
        {
            NotificationContent notificationContent = new()
            {
                Title = title,
                Message = message,
                Type = notificationType
            };

            notificationManager.Show(notificationContent, areaIdentifier, closeOnClick, expirationTime, onClick, onClose);
        }
    }
}