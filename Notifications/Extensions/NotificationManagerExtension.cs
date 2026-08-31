using Notifications.Enums;
using System;

namespace Notifications.Extensions
{
    /// <summary>Provides the legacy title-message-type convenience overload.</summary>
    public static class NotificationManagerExtension
    {
        /// <summary>Displays built-in structured notification content.</summary>
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
