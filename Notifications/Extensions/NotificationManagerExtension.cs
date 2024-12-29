using Notifications.Enums;
using System;

namespace Notifications.Extensions
{
    public static class NotificationManagerExtension
    {
#if NETFRAMEWORK
        public static void Show(this INotificationManager notificationManager,
                                    string title,
                                    string message,
                                    NotificationType notificationType,
                                    string areaIdentifier = "",
                                    bool closeOnClick = true,
                                    TimeSpan? expirationTime = null,
                                    Action onClick = null,
                                    Action onClose = null)
#else
        public static void Show(this INotificationManager notificationManager,
                                    string title,
                                    string message,
                                    NotificationType notificationType,
                                    string areaIdentifier = "",
                                    bool closeOnClick = true,
                                    TimeSpan? expirationTime = null,
                                    Action? onClick = null,
                                    Action? onClose = null)
#endif


        {
            NotificationContent notificationContent = new NotificationContent()
            {
                Title = title,
                Message = message,
                Type = notificationType
            };

            notificationManager.Show(notificationContent, areaIdentifier, closeOnClick, expirationTime, onClick, onClose);
        }
    }
}