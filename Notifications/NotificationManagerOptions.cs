using Notifications.Constants;
using Notifications.Enums;
using System;
using System.Windows;

namespace Notifications
{
    /// <summary>Configures one notification manager without changing process-wide defaults.</summary>
    public sealed class NotificationManagerOptions
    {
        private TimeSpan _defaultExpirationTime = TimeSpan.FromSeconds(5);

        /// <summary>Gets or sets the default expiration interval.</summary>
        public TimeSpan DefaultExpirationTime
        {
            get => _defaultExpirationTime;
            set
            {
                Controls.Notification.ValidateExpirationTime(value, nameof(value));
                _defaultExpirationTime = value;
            }
        }

        /// <summary>Gets or sets whether pointer hover pauses automatic expiration.</summary>
        public bool PauseOnHover { get; set; } = true;

        /// <summary>Gets or sets whether keyboard focus pauses automatic expiration.</summary>
        public bool PauseOnKeyboardFocus { get; set; } = true;

        /// <summary>Gets or sets whether the default template displays a close button.</summary>
        public bool ShowCloseButton { get; set; } = true;

        /// <summary>Gets or sets whether the default template displays an expiration countdown bar.</summary>
        public bool ShowCountdownBar { get; set; } = true;

        /// <summary>Gets or sets desktop-overlay options.</summary>
        public NotificationOverlayOptions Overlay { get; set; } = new NotificationOverlayOptions();

        internal static NotificationManagerOptions FromLegacyDefaults()
        {
            return new NotificationManagerOptions
            {
                Overlay = new NotificationOverlayOptions
                {
                    AllowRemovingPermanentOnOverflow = NotificationConstants.OverlayWindowAllowRemovingPermanentOnOverflow,
                    Margin = NotificationConstants.OverlayWindowMargin,
                    MaxItems = NotificationConstants.OverlayWindowMaxCount,
                    NotificationMargin = NotificationConstants.OverlayWindowNotificationMargin,
                    Position = NotificationConstants.OverlayWindowNotificationPosition,
                    ReverseOrder = NotificationConstants.OverlayWindowReverseOrder,
                },
            };
        }

        internal NotificationManagerOptions Clone()
        {
            var overlay = Overlay ?? throw new InvalidOperationException("Overlay options cannot be null.");
            overlay.Validate();
            return new NotificationManagerOptions
            {
                DefaultExpirationTime = DefaultExpirationTime,
                PauseOnHover = PauseOnHover,
                PauseOnKeyboardFocus = PauseOnKeyboardFocus,
                ShowCloseButton = ShowCloseButton,
                ShowCountdownBar = ShowCountdownBar,
                Overlay = new NotificationOverlayOptions
                {
                    AllowRemovingPermanentOnOverflow = overlay.AllowRemovingPermanentOnOverflow,
                    Margin = overlay.Margin,
                    MaxItems = overlay.MaxItems,
                    NotificationMargin = overlay.NotificationMargin,
                    Position = overlay.Position,
                    ReverseOrder = overlay.ReverseOrder,
                    Topmost = overlay.Topmost,
                },
            };
        }
    }

    /// <summary>Configures compact overlay windows created by a notification manager.</summary>
    public sealed class NotificationOverlayOptions
    {
        /// <summary>Gets or sets whether overflow may evict permanent notifications.</summary>
        public bool AllowRemovingPermanentOnOverflow { get; set; } = true;

        /// <summary>Gets or sets the maximum number of active overlay notifications.</summary>
        public uint MaxItems { get; set; } = 5;

        /// <summary>Gets or sets whether visual stacking order is reversed.</summary>
        public bool ReverseOrder { get; set; }

        /// <summary>Gets or sets the overlay area's outer margin.</summary>
        public Thickness Margin { get; set; } = new Thickness(8);

        /// <summary>Gets or sets the margin applied to each notification.</summary>
        public Thickness NotificationMargin { get; set; } = new Thickness(8, 8, 8, 0);

        /// <summary>Gets or sets the overlay anchor position.</summary>
        public NotificationPosition Position { get; set; } = NotificationPosition.BottomRight;

        /// <summary>Gets or sets whether overlay windows remain above normal windows.</summary>
        public bool Topmost { get; set; } = true;

        internal void Validate()
        {
            if (MaxItems == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxItems), MaxItems, "Overlay MaxItems must be greater than zero.");
            }

            if (!Enum.IsDefined(typeof(NotificationPosition), Position))
            {
                throw new ArgumentOutOfRangeException(nameof(Position), Position, "Unknown notification position.");
            }

            ValidateThickness(Margin, nameof(Margin));
            ValidateThickness(NotificationMargin, nameof(NotificationMargin));
        }

        private static void ValidateThickness(Thickness thickness, string parameterName)
        {
            if (!IsFinite(thickness.Left) || !IsFinite(thickness.Top) || !IsFinite(thickness.Right) || !IsFinite(thickness.Bottom))
            {
                throw new ArgumentOutOfRangeException(parameterName, thickness, "Thickness values must be finite.");
            }
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
