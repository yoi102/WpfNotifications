namespace Notifications
{
    public interface INotificationManager
    {
        Task ShowAsync(object content, string areaName = "", bool closeOnClick = true, TimeSpan? expirationTime = null, Action? onClick = null, Action? onClose = null);
    }
}