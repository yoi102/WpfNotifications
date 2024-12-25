using Notifications.Constants;
using Notifications.Extensions;
using Notifications.Sample.Messages;
using System.Windows;
using System.Windows.Media;

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
            NotificationConstants.DefaultNotificationForeground = new SolidColorBrush(Colors.MistyRose);
            NotificationConstants.DefaultNotificationFontSize = 18;
            NotificationConstants.NotificationWidth = 300;
            NotificationConstants.DefaultNotificationFontWeight = FontWeights.Bold;
        }

        public string NotificationArea
        {
            get
            {
                if (isInWindow.IsChecked == true)
                {
                    return notificationArea.Identifier;
                }
                return "";
            }
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
                    throw new NotSupportedException();
            }

            return message;
        }

        private async void CustomButtonClick(object sender, RoutedEventArgs e)
        {
            var message = RandomCustomMessage();
            await notificationManager.ShowAsync(message, NotificationArea);
        }

        private async void MessageButtonClick(object sender, RoutedEventArgs e)
        {
            await notificationManager.ShowAsync("title", NotificationArea);
        }

        private async void MessageWithTitleButtonClick(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            var type = (Enums.NotificationType)random.Next(0, 4);
            await notificationManager.ShowAsync("title", "message", type, NotificationArea);
        }

        private async void UserControlMessageButtonClick(object sender, RoutedEventArgs e)
        {
            UserControlMessage userControlMessage = new UserControlMessage();

            await notificationManager.ShowAsync(userControlMessage, NotificationArea);
        }

        private async void CustomNotificationButtonClick(object sender, RoutedEventArgs e)
        {
            CustomNotification userControlMessage = new CustomNotification();

            await notificationManager.ShowAsync(userControlMessage, NotificationArea, false, TimeSpan.MaxValue);
        }
    }
}