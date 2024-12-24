using Notifications.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications
{
    public class NotificationContent
    {
        public required string Title { get; set; }
        public required string Message { get; set; }

        public NotificationType Type { get; set; }
    }
}