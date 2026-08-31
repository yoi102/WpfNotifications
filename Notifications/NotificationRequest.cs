using System;
using Notifications.Enums;

namespace Notifications
{
    /// <summary>Describes a notification shown through the advanced API.</summary>
    public sealed class NotificationRequest
    {
        /// <summary>Initializes a request with content.</summary>
        public NotificationRequest(object content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        /// <summary>Gets the content displayed by the notification.</summary>
        public object Content { get; }

        /// <summary>Gets or sets the display target.</summary>
        public NotificationTarget Target { get; set; } = NotificationTarget.Overlay();

        /// <summary>Gets or sets whether clicking notification content closes it. The advanced API defaults to <see langword="false"/>.</summary>
        public bool CloseOnClick { get; set; }

        /// <summary>Gets or sets the expiration interval, or <see langword="null"/> to use the manager default.</summary>
        public TimeSpan? ExpirationTime { get; set; }

        /// <summary>Gets or sets whether to show the close button, or <see langword="null"/> to use the manager default.</summary>
        public bool? ShowCloseButton { get; set; }

        /// <summary>Gets or sets whether to show the expiration countdown bar, or <see langword="null"/> to use the manager default.</summary>
        public bool? ShowCountdownBar { get; set; }

        /// <summary>Gets or sets the callback invoked when clickable content is selected.</summary>
        public Action? OnClick { get; set; }

        /// <summary>Gets or sets the callback invoked after the notification closes.</summary>
        public Action? OnClose { get; set; }

        /// <summary>Gets or sets an optional manager-scoped identifier used for deduplication.</summary>
        public string? Tag { get; set; }

        /// <summary>Gets or sets how an existing notification with the same tag is handled.</summary>
        public NotificationDuplicateBehavior DuplicateBehavior { get; set; } = NotificationDuplicateBehavior.ShowNew;
    }
}
