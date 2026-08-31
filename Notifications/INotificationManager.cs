using System;

namespace Notifications
{
    /// <summary>Defines the source-compatible fire-and-forget notification API.</summary>
    public interface INotificationManager
    {
        /// <summary>Displays content in an overlay or named application area.</summary>
        void Show(object content, string areaIdentifier = "", bool closeOnClick = true, TimeSpan? expirationTime = null, Action? onClick = null, Action? onClose = null);
        /// <summary>Clears an overlay or named application area.</summary>
        void Clear(string areaIdentifier = "");
    }
}
