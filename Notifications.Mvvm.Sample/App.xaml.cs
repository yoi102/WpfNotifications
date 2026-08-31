using Microsoft.Extensions.DependencyInjection;
using Notifications.Mvvm.Sample.Interfaces;
using Notifications.Mvvm.Sample.Services;
using Notifications.Mvvm.Sample.ViewModel;
using System.Windows;

namespace Notifications.Mvvm.Sample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        public ServiceProvider Services { get; }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IAsyncNotificationManager, NotificationManager>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<MainViewModel>();
            return services.BuildServiceProvider();
        }

        /// <inheritdoc />
        protected override void OnExit(ExitEventArgs e)
        {
            Services.Dispose();
            base.OnExit(e);
        }
    }
}
