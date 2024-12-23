using Notifications.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications
{
    public interface INotificationManager
    {
        Task ShowAsync(Notification content, string areaName = "", TimeSpan? expirationTime = null, Action? onClick = null, Action? onClose = null);
    }
}