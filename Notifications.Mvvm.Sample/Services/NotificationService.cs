using Notifications.Enums;
using Notifications.Mvvm.Sample.Interfaces;
using Notifications.Mvvm.Sample.Messages;

namespace Notifications.Mvvm.Sample.Services
{
    internal sealed class NotificationService : INotificationService
    {
        private readonly IAsyncNotificationManager _notificationManager;

        public NotificationService(IAsyncNotificationManager notificationManager)
        {
            _notificationManager = notificationManager;
        }

        public Task ClearAsync(string areaIdentifier = "")
        {
            return _notificationManager.ClearAsync(ToTarget(areaIdentifier));
        }

        public Task ShowCustomNotification1Async(
            string areaIdentifier = "",
            bool closeOnClick = false,
            TimeSpan? expirationTime = null,
            Action? onClick = null,
            Action? onClose = null,
            bool? showCloseButton = null,
            bool? showCountdownBar = null)
        {
            return ShowAsync(
                new CustomNotification1(),
                areaIdentifier,
                closeOnClick,
                expirationTime,
                onClick,
                onClose,
                showCloseButton,
                showCountdownBar);
        }

        public Task ShowCustomNotification2Async(
            string areaIdentifier = "",
            bool closeOnClick = false,
            TimeSpan? expirationTime = null,
            Action? onClick = null,
            Action? onClose = null,
            bool? showCloseButton = null,
            bool? showCountdownBar = null)
        {
            return ShowAsync(
                new CustomNotification2(),
                areaIdentifier,
                closeOnClick,
                expirationTime,
                onClick,
                onClose,
                showCloseButton,
                showCountdownBar);
        }

        public Task ShowDefaultMessageAsync(
            string title,
            string message,
            NotificationType notificationType,
            string areaIdentifier = "",
            bool closeOnClick = false,
            TimeSpan? expirationTime = null,
            Action? onClick = null,
            Action? onClose = null,
            bool? showCloseButton = null,
            bool? showCountdownBar = null)
        {
            return ShowAsync(
                new NotificationContent
                {
                    Title = title,
                    Message = message,
                    Type = notificationType,
                },
                areaIdentifier,
                closeOnClick,
                expirationTime,
                onClick,
                onClose,
                showCloseButton,
                showCountdownBar);
        }

        public Task ShowDefaultMessageAsync(
            string message,
            string areaIdentifier = "",
            bool closeOnClick = false,
            TimeSpan? expirationTime = null,
            Action? onClick = null,
            Action? onClose = null,
            bool? showCloseButton = null,
            bool? showCountdownBar = null)
        {
            return ShowAsync(
                message,
                areaIdentifier,
                closeOnClick,
                expirationTime,
                onClick,
                onClose,
                showCloseButton,
                showCountdownBar);
        }

        public Task ShowUserControlMessageAsync(
            string string1,
            string string2,
            string areaIdentifier = "",
            bool closeOnClick = false,
            TimeSpan? expirationTime = null,
            Action? onClick = null,
            Action? onClose = null,
            bool? showCloseButton = null,
            bool? showCountdownBar = null)
        {
            return ShowAsync(
                new UserControlMessage(string1, string2),
                areaIdentifier,
                closeOnClick,
                expirationTime,
                onClick,
                onClose,
                showCloseButton,
                showCountdownBar);
        }

        private Task<INotificationHandle> ShowAsync(
            object content,
            string areaIdentifier,
            bool closeOnClick,
            TimeSpan? expirationTime,
            Action? onClick,
            Action? onClose,
            bool? showCloseButton,
            bool? showCountdownBar)
        {
            return _notificationManager.ShowAsync(new NotificationRequest(content)
            {
                Target = ToTarget(areaIdentifier),
                CloseOnClick = closeOnClick,
                ExpirationTime = expirationTime,
                OnClick = onClick,
                OnClose = onClose,
                ShowCloseButton = showCloseButton,
                ShowCountdownBar = showCountdownBar,
            });
        }

        private static NotificationTarget ToTarget(string areaIdentifier)
        {
            return string.IsNullOrEmpty(areaIdentifier)
                ? NotificationTarget.Overlay()
                : NotificationTarget.Area(areaIdentifier);
        }
    }
}
