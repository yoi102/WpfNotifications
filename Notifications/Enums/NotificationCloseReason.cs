namespace Notifications.Enums
{
    /// <summary>Describes why a notification was dismissed.</summary>
    public enum NotificationCloseReason
    {
        /// <summary>Closed explicitly through a handle or control method.</summary>
        Programmatic,
        /// <summary>Closed by pointer, keyboard, or close button input.</summary>
        User,
        /// <summary>Closed after its expiration interval.</summary>
        Expired,
        /// <summary>Closed by clearing its target.</summary>
        Cleared,
        /// <summary>Closed because the area's item limit was exceeded.</summary>
        Overflow,
        /// <summary>Closed because a tagged replacement was shown.</summary>
        Replaced,
        /// <summary>Closed because its owning manager was disposed.</summary>
        ManagerDisposed,
    }
}
