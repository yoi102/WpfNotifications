using System;
using System.Windows;

namespace Notifications
{
    /// <summary>Selects where an advanced notification is displayed.</summary>
    public sealed class NotificationTarget
    {
        private NotificationTarget(string? areaIdentifier, NotificationMonitor monitor, Window? owner)
        {
            AreaIdentifier = areaIdentifier;
            Monitor = monitor;
            Owner = owner;
        }

        /// <summary>Gets the application-area identifier, or <see langword="null"/> for an overlay target.</summary>
        public string? AreaIdentifier { get; }

        /// <summary>Gets whether this target displays in a desktop overlay.</summary>
        public bool IsOverlay => AreaIdentifier is null;

        /// <summary>Gets the monitor selection used by an overlay target.</summary>
        public NotificationMonitor Monitor { get; }

        /// <summary>Gets the owner window used by an owner-monitor target.</summary>
        public Window? Owner { get; }

        /// <summary>Creates a desktop-overlay target.</summary>
        public static NotificationTarget Overlay(NotificationMonitor monitor = NotificationMonitor.Primary, Window? owner = null)
        {
            if (!Enum.IsDefined(typeof(NotificationMonitor), monitor))
            {
                throw new ArgumentOutOfRangeException(nameof(monitor), monitor, "Unknown monitor selection.");
            }

            if (monitor == NotificationMonitor.Owner && owner is null)
            {
                throw new ArgumentNullException(nameof(owner), "An owner window is required for the Owner monitor target.");
            }

            if (monitor != NotificationMonitor.Owner && owner != null)
            {
                throw new ArgumentException("An owner window can only be supplied with the Owner monitor target.", nameof(owner));
            }

            return new NotificationTarget(null, monitor, owner);
        }

        /// <summary>Creates a target for a uniquely named, loaded application area.</summary>
        public static NotificationTarget Area(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("An area identifier cannot be empty.", nameof(identifier));
            }

            return new NotificationTarget(identifier, NotificationMonitor.Primary, null);
        }
    }

    /// <summary>Selects the monitor used for an overlay notification.</summary>
    public enum NotificationMonitor
    {
        /// <summary>Use the Windows primary monitor.</summary>
        Primary,
        /// <summary>Use the monitor containing the mouse pointer.</summary>
        MousePointer,
        /// <summary>Use the monitor containing the supplied owner window.</summary>
        Owner,
    }
}
