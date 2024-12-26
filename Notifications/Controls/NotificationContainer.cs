using System.Windows.Controls;

namespace Notifications.Controls
{
    public class NotificationContainer : ContentControl
    {
        private readonly Notification notification;

        public NotificationContainer(Notification notification)
        {
            Content = notification;
            this.notification = notification;
        }
        public bool IsPermanent => notification.IsPermanent;
        public bool IsClosing => notification.IsClosing;
        public Notification Notification => notification;
    }
}