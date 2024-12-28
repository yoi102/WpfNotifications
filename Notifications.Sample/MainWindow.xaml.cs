using Notifications.Constants;
using Notifications.Enums;
using Notifications.Extensions;
using Notifications.Sample.Messages;
using System.Windows;
using System.Windows.Controls;
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
            NotificationConstants.OverlayWindowNotificationPosition = Enums.NotificationPosition.TopRight;
            NotificationConstants.OverlayWindowMaxCount = 7;
            NotificationConstants.DefaultNotificationForeground = new SolidColorBrush(Colors.MistyRose);
            NotificationConstants.DefaultNotificationFontSize = 18;
            NotificationConstants.NotificationWidth = 300;
            NotificationConstants.DefaultNotificationFontWeight = FontWeights.Bold;
            NotificationConstants.OverlayWindowAllowRemovingPermanentOnOverflow = false;
            NotificationConstants.OverlayWindowReverseOrder = false;
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

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            notificationManager.Clear(NotificationArea);
        }

        private void comb_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            if (notificationArea == null)
            {
                return;
            }
            notificationArea.Position = (NotificationPosition)comboBox.SelectedIndex;
        }

        private void CustomButtonClick(object sender, RoutedEventArgs e)
        {
            var message = RandomCustomMessage();
            notificationManager.Show(message, NotificationArea);
        }

        private void CustomNotificationButtonClick(object sender, RoutedEventArgs e)
        {
            CustomNotification userControlMessage = new CustomNotification();

            notificationManager.Show(userControlMessage, NotificationArea, false, TimeSpan.MaxValue);
        }

        private void MessageButtonClick(object sender, RoutedEventArgs e)
        {
            notificationManager.Show("title", NotificationArea);
        }

        private void MessageWithTitleButtonClick(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            var type = (Enums.NotificationType)random.Next(0, 4);
            notificationManager.Show("title", "message", type, NotificationArea);
        }

        private void UserControlMessageButtonClick(object sender, RoutedEventArgs e)
        {
            UserControlMessage userControlMessage = new UserControlMessage();

            notificationManager.Show(userControlMessage, NotificationArea);
        }
    }
}