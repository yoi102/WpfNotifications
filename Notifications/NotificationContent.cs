using Notifications.Enums;

namespace Notifications
{
    /// <summary>Represents the title, message, and semantic type used by the built-in template.</summary>
    public class NotificationContent
    {
        /// <summary>Gets or sets the notification title.</summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>Gets or sets the notification message.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Gets or sets the semantic notification type.</summary>
        public NotificationType Type { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Title) ? Message : $"{Title}: {Message}";
        }
    }
}
