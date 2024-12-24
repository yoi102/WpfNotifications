using Notifications.Enums;
using System.Windows;
using System.Windows.Media;

namespace Notifications.Constants
{
    public class NotificationConstants
    {
        /// <summary> work area height </summary>
        public static double AreaHeight => SystemParameters.WorkArea.Height;

        /// <summary> work area width </summary>
        public static double AreaWidth => SystemParameters.WorkArea.Width;

        /// <summary> Overlay window maximum count </summary>
        public static uint NotificationsOverlayWindowMaxCount { get; set; } = uint.MaxValue;

        public static Thickness NotificationsOverlayWindowNotificationMargin { get; set; } = new Thickness(0, 12, 0, 0);

        /// <summary> Overlay message position </summary>
        public static NotificationPosition NotificationsOverlayWindowNotificationPosition { get; set; } = NotificationPosition.BottomRight;
        public static double NotificationWidth { get; set; } = 350;
        public static double NotificationFontSize { get; set; } = 15;
        public static FontWeight NotificationFontWeight { get; set; } = FontWeights.Normal;
        public static Brush NotificationBackground { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444444"));
        public static Brush NotificationForeground { get; set; } = new SolidColorBrush(Colors.White);
   
        
    }
}