using Notifications.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Extensions
{
    public static class NotificationManagerExtension
    {

        public static async Task ShowAsync(this NotificationManager notificationManager,
                                    string title,
                                    string message,
                                    NotificationType notificationType,
                                    string areaIdentifier = "",
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

            await notificationManager.ShowAsync(notificationContent, areaIdentifier, expirationTime, onClick, onClose);
        }


        public static async Task ShowAsync(this NotificationManager notificationManager,
                            string message,
                            string areaIdentifier = "",
                            TimeSpan? expirationTime = null,
                            Action? onClick = null,
                            Action? onClose = null)
        {
            await notificationManager.ShowAsync(message, areaIdentifier, expirationTime, onClick, onClose);
        }





    }
}
