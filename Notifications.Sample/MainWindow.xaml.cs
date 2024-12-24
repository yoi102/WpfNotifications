using Notifications.Constants;
using Notifications.Sample.Messages;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Notifications.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NotificationManager notificationManager;

        public MainWindow()
        {
            InitializeComponent();

            notificationManager = new NotificationManager();
            NotificationConstants.NotificationsOverlayWindowNotificationPosition = Enums.NotificationPosition.TopRight;
            NotificationConstants.NotificationsOverlayWindowMaxCount = 7;
            NotificationConstants.NotificationForeground = new SolidColorBrush(Colors.Cornsilk);
            NotificationConstants.NotificationFontSize = 18;
            NotificationConstants.NotificationWidth = 300;
            NotificationConstants.NotificationFontWeight = FontWeights.Bold;
        }

        private static object RandomCustomMessage()
        {
            object message;
            Random random = new Random();
            var type = random.Next(0, 4);
            switch (type)
            {
                case 0:
                    message = new InformationMessage();
                    break;

                case 1:
                    message = new SuccessMessage();
                    break;

                case 2:
                    message = new WarningMessage();
                    break;

                case 3:
                    message = new ErrorMessage();
                    break;

                default:
                    throw new ApplicationException();
            }

            return message;
        }

        private async void CustomButtonClick(object sender, RoutedEventArgs e)
        {
            var message = RandomCustomMessage();
            await notificationManager.ShowAsync(message);
        }

        private async void CustomInWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var message = RandomCustomMessage();
            await notificationManager.ShowAsync(message, "main_window");
        }
        private async void MessageButtonClick(object sender, RoutedEventArgs e)
        {
            await notificationManager.ShowAsync("title");
        }

        private async void MessageWithTitleButtonClick(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            var type = (Enums.NotificationType)random.Next(0, 4);
            await notificationManager.ShowAsync("title", "message", type);
        }
    }
}