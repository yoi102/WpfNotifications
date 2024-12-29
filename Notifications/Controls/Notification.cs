using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Notifications.Controls
{
    [TemplatePart(Name = "PART_CountdownBar", Type = typeof(Rectangle))]
    public class Notification : ContentControl
    {
        // Using a DependencyProperty as the backing store for CountdownBarFill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountdownBarFillProperty =
            DependencyProperty.Register("CountdownBarFill", typeof(Brush), typeof(Notification), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        // Using a DependencyProperty as the backing store for ExpirationTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpirationTimeProperty =
            DependencyProperty.Register("ExpirationTime", typeof(Duration), typeof(Notification), new PropertyMetadata(new Duration(TimeSpan.FromSeconds(1))));

        // Using a DependencyProperty as the backing store for IsPermanent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPermanentProperty =
            DependencyProperty.Register("IsPermanent", typeof(bool), typeof(Notification), new PropertyMetadata(false));

        public static readonly RoutedEvent NotificationClosedEvent = EventManager.RegisterRoutedEvent(
                            "NotificationClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        public static readonly RoutedEvent NotificationCloseInvokedEvent = EventManager.RegisterRoutedEvent(
            "NotificationCloseInvoked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        public static readonly RoutedEvent NotificationClosingEvent = EventManager.RegisterRoutedEvent(
           "NotificationClosing", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        private TimeSpan _closingAnimationTime = TimeSpan.Zero;

        private Rectangle _countdownBar = null!;

        public event RoutedEventHandler NotificationClosed
        {
            add { AddHandler(NotificationClosedEvent, value); }
            remove { RemoveHandler(NotificationClosedEvent, value); }
        }

        public event RoutedEventHandler NotificationCloseInvoked
        {
            add { AddHandler(NotificationCloseInvokedEvent, value); }
            remove { RemoveHandler(NotificationCloseInvokedEvent, value); }
        }

        public event RoutedEventHandler NotificationClosing
        {
            add { AddHandler(NotificationClosingEvent, value); }
            remove { RemoveHandler(NotificationClosingEvent, value); }
        }

        public Brush CountdownBarFill
        {
            get { return (Brush)GetValue(CountdownBarFillProperty); }
            set { SetValue(CountdownBarFillProperty, value); }
        }

        public Duration ExpirationTime
        {
            get { return (Duration)GetValue(ExpirationTimeProperty); }
            set { SetValue(ExpirationTimeProperty, value); }
        }

        public bool IsClosing { get; set; }

        public bool IsPermanent
        {
            get { return (bool)GetValue(IsPermanentProperty); }
            set { SetValue(IsPermanentProperty, value); }
        }

        public async void Close()
        {
            if (IsClosing)
            {
                return;
            }
            IsClosing = true;

            RaiseEvent(new RoutedEventArgs(NotificationCloseInvokedEvent));
            await Task.Delay(_closingAnimationTime);
            RaiseEvent(new RoutedEventArgs(NotificationClosedEvent));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _countdownBar = (Rectangle)GetTemplateChild("PART_CountdownBar");

            var triggers = Style.Triggers;
            if (triggers.Count == 0)
            {
                triggers = Template.Triggers;
            }
            var closingStoryboards = triggers.OfType<EventTrigger>().FirstOrDefault(t => t.RoutedEvent == NotificationCloseInvokedEvent)?.Actions.OfType<BeginStoryboard>().Select(a => a.Storyboard);
            _closingAnimationTime = new TimeSpan(closingStoryboards?.Max(s => Math.Min((s.Duration.HasTimeSpan ? s.Duration.TimeSpan + (s.BeginTime ?? TimeSpan.Zero) : TimeSpan.MaxValue).Ticks, s.Children.Select(ch => ch.Duration.TimeSpan + (s.BeginTime ?? TimeSpan.Zero)).Max().Ticks)) ?? 0);
        }

        public virtual async void ScheduleClose(TimeSpan expirationTime)
        {
            ExpirationTime = expirationTime;
            RaiseEvent(new RoutedEventArgs(NotificationClosingEvent));

            if (expirationTime == TimeSpan.MaxValue)
            {
                IsPermanent = true;
                return;
            }

            if (Content is string || Content is NotificationContent)
            {
                await ShrinkColorBarOverTimeAsync(expirationTime);
            }
            await Task.Delay(expirationTime);

            Close();
        }

        private async Task ShrinkColorBarOverTimeAsync(TimeSpan expirationTime)
        {
            for (int i = 0; i < 100; i++)
            {
                if (_countdownBar is not null)
                    break;
                await Task.Delay(10);
            }

            if (_countdownBar is null)
                return;
            _countdownBar.Height = 5;
            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                From = _countdownBar.RenderSize.Width,
                To = 0,
                Duration = new Duration(expirationTime),
                EasingFunction = new QuadraticEase()
            };

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(widthAnimation);

            Storyboard.SetTarget(widthAnimation, _countdownBar);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(FrameworkElement.WidthProperty));

            storyboard.Begin();
        }
    }
}