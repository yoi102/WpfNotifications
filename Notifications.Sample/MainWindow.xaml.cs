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
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            SuccessMessage message = new SuccessMessage();
            await notificationManager.ShowAsync(message, "aasdssd");
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ErrorMessage message = new ErrorMessage();
            await notificationManager.ShowAsync(message);

        }
    }
}