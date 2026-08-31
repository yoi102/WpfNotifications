using Notifications.Controls;
using Notifications.Enums;
using Notifications.Extensions;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Threading.Tasks;

namespace Notifications
{
    /// <summary>Legacy-visible desktop overlay window implementation.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class NotificationsOverlayWindow : Window
    {
        private readonly NotificationPosition _position;
        private readonly Rect _workArea;
        private bool _hasShownNotification;

        /// <summary>Initializes an overlay window using legacy process-wide defaults.</summary>
        public NotificationsOverlayWindow()
            : this(NotificationManagerOptions.FromLegacyDefaults().Overlay, SystemParameters.WorkArea)
        {
        }

        internal NotificationsOverlayWindow(NotificationOverlayOptions options, Rect workArea)
        {
            InitializeComponent();
            _position = options.Position;
            _workArea = workArea;
            Topmost = options.Topmost;
            MaxHeight = workArea.Height;
            MaxWidth = workArea.Width;
            area.AllowRemovingPermanentOnOverflow = options.AllowRemovingPermanentOnOverflow;
            area.Margin = options.Margin;
            area.MaxItems = options.MaxItems;
            area.NotificationMargin = options.NotificationMargin;
            area.Position = options.Position;
            area.ReverseOrder = options.ReverseOrder;
            area.ItemsChanged += Area_ItemsChanged;
            SizeChanged += (_, _) => UpdatePosition();
        }

        /// <summary>Starts closing all notifications in this overlay window.</summary>
        public void Clear()
        {
            area.Clear();
        }

        internal Task ClearAsync()
        {
            return area.ClearAsync(NotificationCloseReason.Cleared);
        }

        internal Notification? ShowNotification(
            object content,
            bool closeOnClick,
            TimeSpan expirationTime,
            Action? onClick,
            Action? onClose,
            NotificationDisplayOptions displayOptions)
        {
            _hasShownNotification = true;
            return area.ShowManaged(
                content,
                closeOnClick,
                expirationTime,
                onClick,
                onClose,
                displayOptions);
        }

        internal Notification? ShowNotification(
            object content,
            bool closeOnClick,
            TimeSpan expirationTime,
            Action? onClick,
            Action? onClose,
            NotificationManagerOptions options)
        {
            return ShowNotification(
                content,
                closeOnClick,
                expirationTime,
                onClick,
                onClose,
                NotificationDisplayOptions.FromManager(options));
        }

        /// <inheritdoc />
        protected override void OnClosed(EventArgs e)
        {
            area.ItemsChanged -= Area_ItemsChanged;
            base.OnClosed(e);
        }

        private void NotificationsOverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            var exStyle = (int)handle.GetWindowLong((int)WindowExtensions.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)(WindowExtensions.ExtendedWindowStyles.WS_EX_TOOLWINDOW | WindowExtensions.ExtendedWindowStyles.WS_EX_NOACTIVATE);
            handle.SetWindowLong((int)WindowExtensions.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
            UpdatePosition();
        }

        private void Area_ItemsChanged(object? sender, EventArgs e)
        {
            if (_hasShownNotification && area.NotificationCount == 0 && IsLoaded)
            {
                Close();
            }
        }

        private void UpdatePosition()
        {
            if (ActualWidth <= 0 || ActualHeight <= 0)
            {
                return;
            }

            Left = IsLeft(_position) ? _workArea.Left : IsCenterHorizontal(_position) ? _workArea.Left + ((_workArea.Width - ActualWidth) / 2) : _workArea.Right - ActualWidth;
            Top = IsTop(_position) ? _workArea.Top : IsCenterVertical(_position) ? _workArea.Top + ((_workArea.Height - ActualHeight) / 2) : _workArea.Bottom - ActualHeight;
        }

        private static bool IsLeft(NotificationPosition position) => position == NotificationPosition.TopLeft || position == NotificationPosition.BottomLeft || position == NotificationPosition.CenterLeft;
        private static bool IsCenterHorizontal(NotificationPosition position) => position == NotificationPosition.TopCenter || position == NotificationPosition.BottomCenter || position == NotificationPosition.Center;
        private static bool IsTop(NotificationPosition position) => position == NotificationPosition.TopLeft || position == NotificationPosition.TopRight || position == NotificationPosition.TopCenter;
        private static bool IsCenterVertical(NotificationPosition position) => position == NotificationPosition.CenterLeft || position == NotificationPosition.CenterRight || position == NotificationPosition.Center;
    }
}
