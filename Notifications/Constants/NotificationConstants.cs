using Notifications.Enums;
using System.Windows;
using System.Windows.Media;

namespace Notifications.Constants
{
    public class NotificationConstants
    {

        public static Brush StringNotificationBackground { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444444"));

        public static double DefaultNotificationFontSize { get; set; } = 15;

        public static FontWeight DefaultNotificationFontWeight { get; set; } = FontWeights.Normal;

        public static Brush DefaultNotificationForeground { get; set; } = new SolidColorBrush(Colors.White);

        /// <summary> Overlay window maximum count </summary>
        public static bool OverlayWindowAllowRemovingPermanentOnOverflow { get; set; } = true;
        public static uint OverlayWindowMaxCount { get; set; } = uint.MaxValue;
        public static bool OverlayWindowReverseOrder { get; set; } = false;

        public static Thickness OverlayWindowMargin { get; set; } = new Thickness(0, 0, 0, 0);
        public static Thickness OverlayWindowNotificationMargin { get; set; } = new Thickness(8, 8, 8, 0);

        /// <summary> Overlay message position </summary>
        public static NotificationPosition OverlayWindowNotificationPosition { get; set; } = NotificationPosition.BottomRight;

        public static double NotificationWidth { get; set; } = 350;
    }
}