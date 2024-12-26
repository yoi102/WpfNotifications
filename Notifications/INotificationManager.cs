namespace Notifications
{
    public interface INotificationManager
    {
        void Show(object content, string areaName = "", bool closeOnClick = true, TimeSpan? expirationTime = null, Action? onClick = null, Action? onClose = null);
        void Clear(string areaIdentifier = "");

    }
}