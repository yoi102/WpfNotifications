using Notifications.Controls;

namespace Notifications
{
    /// <summary>Configures interaction and optional controls for a displayed notification.</summary>
    public sealed class NotificationDisplayOptions
    {
        /// <summary>Gets or sets whether pointer hover pauses automatic expiration.</summary>
        public bool PauseOnHover { get; set; } = true;

        /// <summary>Gets or sets whether keyboard focus pauses automatic expiration.</summary>
        public bool PauseOnKeyboardFocus { get; set; } = true;

        /// <summary>Gets or sets whether the default template displays a close button.</summary>
        public bool ShowCloseButton { get; set; } = true;

        /// <summary>Gets or sets whether the default template displays an expiration countdown bar.</summary>
        public bool ShowCountdownBar { get; set; } = true;

        internal static NotificationDisplayOptions FromManager(
            NotificationManagerOptions options,
            NotificationRequest? request = null)
        {
            return new NotificationDisplayOptions
            {
                PauseOnHover = options.PauseOnHover,
                PauseOnKeyboardFocus = options.PauseOnKeyboardFocus,
                ShowCloseButton = request?.ShowCloseButton ?? options.ShowCloseButton,
                ShowCountdownBar = request?.ShowCountdownBar ?? options.ShowCountdownBar,
            };
        }

        internal void ApplyTo(Notification notification)
        {
            notification.PauseOnHover = PauseOnHover;
            notification.PauseOnKeyboardFocus = PauseOnKeyboardFocus;
            notification.ShowCloseButton = ShowCloseButton;
            notification.ShowCountdownBar = ShowCountdownBar;
        }
    }
}
