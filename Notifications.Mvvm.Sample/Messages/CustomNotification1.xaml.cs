using System.ComponentModel;
using System.Windows;

namespace Notifications.Mvvm.Sample.Messages
{
    /// <summary>
    /// CustomNotification.xaml 的交互逻辑
    /// </summary>
    public partial class CustomNotification1 : INotifyPropertyChanged
    {
        private int progress;

        public CustomNotification1()
        {
            InitializeComponent();
            DataContext = this;

            var progress = new Progress<int>(value =>
            {
                Progress = value;
            });

            Task.Run(async () => await RunTask(progress));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));

                if (value == 100)
                {
                    img.Visibility = Visibility.Collapsed;
                }
            }
        }

        public async Task RunTask(IProgress<int> progress)
        {
            for (int i = 0; i < 101; i++)
            {
                await Task.Delay(100);
                progress.Report(i);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}