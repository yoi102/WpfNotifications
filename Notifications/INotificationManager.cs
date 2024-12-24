namespace Notifications
{
    public interface INotificationManager
    {
        Task ShowAsync(object content, string areaName = "", TimeSpan? expirationTime = null, Action? onClick = null, Action? onClose = null);
    }
}