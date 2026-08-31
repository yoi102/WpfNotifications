using System.Windows.Controls;

namespace Notifications.Controls
{
    /// <summary>Hosts a notification inside a <see cref="NotificationArea"/> template.</summary>
    public class NotificationContainer : ContentControl
    {
        private readonly Notification notification;

        /// <summary>Initializes a container for a notification.</summary>
        public NotificationContainer(Notification notification)
        {
            Content = notification;
            this.notification = notification;
        }
        /// <summary>Gets whether the notification has no automatic expiration.</summary>
        public bool IsPermanent => notification.IsPermanent;

        /// <summary>Gets whether the notification is currently closing.</summary>
        public bool IsClosing => notification.IsClosing;

        /// <summary>Gets the hosted notification.</summary>
        public Notification Notification => notification;
    }
}
