using Notifications.Enums;
using System;
using System.Windows;
using System.Windows.Media;

namespace Notifications.Constants
{
    /// <summary>Provides process-wide legacy defaults and theme resource keys.</summary>
    public class NotificationConstants
    {
        /// <summary>Resource key for the background brush used by string notifications.</summary>
        public const string StringNotificationBackgroundResourceKey = "Notifications.StringNotificationBackground";
        /// <summary>Resource key for the default notification font size.</summary>
        public const string DefaultNotificationFontSizeResourceKey = "Notifications.DefaultNotificationFontSize";
        /// <summary>Resource key for the default notification font weight.</summary>
        public const string DefaultNotificationFontWeightResourceKey = "Notifications.DefaultNotificationFontWeight";
        /// <summary>Resource key for the default notification foreground brush.</summary>
        public const string DefaultNotificationForegroundResourceKey = "Notifications.DefaultNotificationForeground";

        private static Brush _stringNotificationBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444444"));
        private static double _defaultNotificationFontSize = 15;
        private static FontWeight _defaultNotificationFontWeight = FontWeights.Normal;
        private static Brush _defaultNotificationForeground = Brushes.White;
        private static double _notificationWidth = 350;
        private static bool _backgroundConfigured;
        private static bool _fontSizeConfigured;
        private static bool _fontWeightConfigured;
        private static bool _foregroundConfigured;

        /// <summary>Gets or sets the process-wide background brush for string notifications.</summary>
        public static Brush StringNotificationBackground
        {
            get => _stringNotificationBackground;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _stringNotificationBackground = value;
                _backgroundConfigured = true;
                UpdateApplicationResource(StringNotificationBackgroundResourceKey, value);
            }
        }

        /// <summary>Gets or sets the process-wide default notification font size.</summary>
        public static double DefaultNotificationFontSize
        {
            get => _defaultNotificationFontSize;
            set
            {
                ValidatePositiveFinite(value, nameof(value));
                _defaultNotificationFontSize = value;
                _fontSizeConfigured = true;
                UpdateApplicationResource(DefaultNotificationFontSizeResourceKey, value);
            }
        }

        /// <summary>Gets or sets the process-wide default notification font weight.</summary>
        public static FontWeight DefaultNotificationFontWeight
        {
            get => _defaultNotificationFontWeight;
            set
            {
                _defaultNotificationFontWeight = value;
                _fontWeightConfigured = true;
                UpdateApplicationResource(DefaultNotificationFontWeightResourceKey, value);
            }
        }

        /// <summary>Gets or sets the process-wide default notification foreground brush.</summary>
        public static Brush DefaultNotificationForeground
        {
            get => _defaultNotificationForeground;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _defaultNotificationForeground = value;
                _foregroundConfigured = true;
                UpdateApplicationResource(DefaultNotificationForegroundResourceKey, value);
            }
        }

        /// <summary>Gets or sets whether legacy overlay defaults may evict permanent notifications.</summary>
        public static bool OverlayWindowAllowRemovingPermanentOnOverflow { get; set; } = true;
        /// <summary>Gets or sets the legacy overlay item limit.</summary>
        public static uint OverlayWindowMaxCount { get; set; } = 5;
        /// <summary>Gets or sets the legacy overlay reverse-order setting.</summary>
        public static bool OverlayWindowReverseOrder { get; set; }
        /// <summary>Gets or sets the legacy overlay outer margin.</summary>
        public static Thickness OverlayWindowMargin { get; set; } = new Thickness();
        /// <summary>Gets or sets the legacy margin applied to each overlay notification.</summary>
        public static Thickness OverlayWindowNotificationMargin { get; set; } = new Thickness(8, 8, 8, 0);
        /// <summary>Gets or sets the legacy overlay anchor position.</summary>
        public static NotificationPosition OverlayWindowNotificationPosition { get; set; } = NotificationPosition.BottomRight;
        /// <summary>Gets or sets the width assigned to non-visual notification content.</summary>
        public static double NotificationWidth
        {
            get => _notificationWidth;
            set
            {
                ValidatePositiveFinite(value, nameof(value));
                _notificationWidth = value;
            }
        }

        internal static void ApplyConfiguredApplicationResources()
        {
            if (_backgroundConfigured)
            {
                UpdateApplicationResource(StringNotificationBackgroundResourceKey, _stringNotificationBackground);
            }

            if (_fontSizeConfigured)
            {
                UpdateApplicationResource(DefaultNotificationFontSizeResourceKey, _defaultNotificationFontSize);
            }

            if (_fontWeightConfigured)
            {
                UpdateApplicationResource(DefaultNotificationFontWeightResourceKey, _defaultNotificationFontWeight);
            }

            if (_foregroundConfigured)
            {
                UpdateApplicationResource(DefaultNotificationForegroundResourceKey, _defaultNotificationForeground);
            }
        }

        private static void UpdateApplicationResource(string key, object value)
        {
            var application = Application.Current;
            if (application is null)
            {
                return;
            }

            if (application.Dispatcher.CheckAccess())
            {
                application.Resources[key] = value;
                return;
            }

            application.Dispatcher.Invoke(() => application.Resources[key] = value);
        }

        private static void ValidatePositiveFinite(double value, string parameterName)
        {
            if (value <= 0 || double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Value must be positive and finite.");
            }
        }
    }
}
