using Notifications.Enums;

namespace Notifications.Mvvm.Sample.Interfaces
{
    internal interface INotificationService
    {
        Task ClearAsync(string areaIdentifier = "");

        Task ShowCustomNotification1Async(
            string areaIdentifier = "",
            bool closeOnClick = false,
            TimeSpan? expirationTime = null,
            Action? onClick = null,
            Action? onClose = null,
            bool? showCloseButton = null,
            bool? showCountdownBar = null);

        Task ShowCustomNotification2Async(
            string areaIdentifier = "",
            bool closeOnClick = false,
            TimeSpan? expirationTime = null,
            Action? onClick = null,
            Action? onClose = null,
            bool? showCloseButton = null,
            bool? showCountdownBar = null);

        Task ShowDefaultMessageAsync(
            string title,
            string message,
            NotificationType notificationType,
            string areaIdentifier = "",
            bool closeOnClick = false,
            TimeSpan? expirationTime = null,
            Action? onClick = null,
            Action? onClose = null,
            bool? showCloseButton = null,
            bool? showCountdownBar = null);

        Task ShowDefaultMessageAsync(
            string message,
            string areaIdentifier = "",
            bool closeOnClick = false,
            TimeSpan? expirationTime = null,
            Action? onClick = null,
            Action? onClose = null,
            bool? showCloseButton = null,
            bool? showCountdownBar = null);

        Task ShowUserControlMessageAsync(
            string string1,
            string string2,
            string areaIdentifier = "",
            bool closeOnClick = false,
            TimeSpan? expirationTime = null,
            Action? onClick = null,
            Action? onClose = null,
            bool? showCloseButton = null,
            bool? showCountdownBar = null);
    }
}
