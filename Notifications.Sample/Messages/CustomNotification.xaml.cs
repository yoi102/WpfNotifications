﻿using System.ComponentModel;
using System.Windows;

namespace Notifications.Sample.Messages
{
    /// <summary>
    /// CustomNotification.xaml 的交互逻辑
    /// </summary>
    public partial class CustomNotification : INotifyPropertyChanged
    {
        private int progress;

        public CustomNotification()
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await CloseAsync();
        }
    }
}