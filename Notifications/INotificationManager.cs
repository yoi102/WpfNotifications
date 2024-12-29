using System;

namespace Notifications
{
    public interface INotificationManager
    {
#if NETFRAMEWORK
        void Show(object content, string areaName = "", bool closeOnClick = true, TimeSpan? expirationTime = null, Action onClick = null, Action onClose = null);

#else
        void Show(object content, string areaName = "", bool closeOnClick = true, TimeSpan? expirationTime = null, Action? onClick = null, Action? onClose = null);
#endif
        void Clear(string areaIdentifier = "");

    }
}