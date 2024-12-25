using Notifications.Enums;

namespace Notifications
{
    public class NotificationContent
    {
        public required string Title { get; set; }
        public required string Message { get; set; }

        public NotificationType Type { get; set; }
    }
}