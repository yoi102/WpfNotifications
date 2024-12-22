using Notifications.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Notifications.Constants
{
    public class NotificationConstants
    {

        /// <summary> Overlay window maximum count </summary>
        public static uint NotificationsOverlayWindowMaxCount { get; set; } = uint.MaxValue;
        public static Thickness NotificationsOverlayWindowNotificationMargin { get; set; } = new Thickness(0,12,0,0);

        /// <summary> Overlay message position </summary>
        public static NotificationPosition NotificationsOverlayWindowNotificationPosition { get; set; } = NotificationPosition.BottomRight;

        /// <summary> work area height </summary>
        public static double AreaHeight => SystemParameters.WorkArea.Height;
        /// <summary> work area width </summary>
        public static double AreaWidth => SystemParameters.WorkArea.Width;







    }
}
