namespace Notifications.Enums
{
    /// <summary>Controls how a manager handles a second active request with the same tag.</summary>
    public enum NotificationDuplicateBehavior
    {
        /// <summary>Always display a new notification.</summary>
        ShowNew,
        /// <summary>Keep and return the existing tagged notification.</summary>
        Ignore,
        /// <summary>Update the existing notification's content and expiration.</summary>
        UpdateExisting,
        /// <summary>Close the existing notification and display a replacement.</summary>
        Replace,
    }
}
