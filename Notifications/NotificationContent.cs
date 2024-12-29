using Notifications.Enums;

namespace Notifications
{
    public class NotificationContent
    {
        public string Title { get; set; } = string.Empty;
        public  string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }
    }
}