using Microsoft.Extensions.DependencyInjection;
using Notifications.Mvvm.Sample.ViewModel;
using System.Windows;

namespace Notifications.Mvvm.Sample.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<MainViewModel>();
        }
    }
}