using Notifications.Enums;

namespace Notifications.Mvvm.Sample.Interfaces
{
    internal interface INotificationService
    {
        void Clear(string areaIdentifier = "");

        void ShowCustomNotification1(string areaIdentifier = "",
                                                    bool closeOnClick = true,
                                            TimeSpan? expirationTime = null,
                                            Action? onClick = null,
                                            Action? onClose = null);

        void ShowCustomNotification2(string areaIdentifier = "",
                                            bool closeOnClick = true,
                                            TimeSpan? expirationTime = null,
                                            Action? onClick = null,
                                            Action? onClose = null);

        void ShowDefaultMessage(string title,
                                string message,
                                NotificationType notificationType,
                                string areaIdentifier = "",
                                bool closeOnClick = true,
                                TimeSpan? expirationTime = null,
                                Action? onClick = null,
                                Action? onClose = null);

        void ShowDefaultMessage(string message,
                                string areaIdentifier = "",
                                bool closeOnClick = true,
                                TimeSpan? expirationTime = null,
                                Action? onClick = null,
                                Action? onClose = null);

        void ShowUserControlMessage(string string1,
                                    string string2,
                                    string areaIdentifier = "",
                                    bool closeOnClick = true,
                                    TimeSpan? expirationTime = null,
                                    Action? onClick = null,
                                    Action? onClose = null);
    }
}