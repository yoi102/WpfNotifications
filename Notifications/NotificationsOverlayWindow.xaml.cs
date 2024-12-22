using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
