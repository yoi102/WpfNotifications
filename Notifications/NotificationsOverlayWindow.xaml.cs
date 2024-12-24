using Notifications.Extensions;
using System.Windows;
using System.Windows.Interop;

namespace Notifications
{
    /// <summary>
    /// NotificationsOverlayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NotificationsOverlayWindow : Window
    {
        public NotificationsOverlayWindow()
        {
            InitializeComponent();
        }

        private void NotificationsOverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)wndHelper.Handle.GetWindowLong((int)WindowExtensions.GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)WindowExtensions.ExtendedWindowStyles.WS_EX_TOOLWINDOW;

            wndHelper.Handle.SetWindowLong((int)WindowExtensions.GetWindowLongFields.GWL_EXSTYLE, exStyle);
        }
    }
}