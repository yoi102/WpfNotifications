using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Notifications.Controls
{
    [TemplatePart(Name = "PART_ColorBar", Type = typeof(Rectangle))]
    public class Notification : ContentControl
    {
        // Using a DependencyProperty as the backing store for ExpirationTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpirationTimeProperty =
            DependencyProperty.Register("ExpirationTime", typeof(Duration), typeof(Notification), new PropertyMetadata(new Duration(TimeSpan.FromSeconds(1))));

        public static readonly RoutedEvent NotificationClosedEvent = EventManager.RegisterRoutedEvent(
                    "NotificationClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        public static readonly RoutedEvent NotificationCloseInvokedEvent = EventManager.RegisterRoutedEvent(
            "NotificationCloseInvoked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        public static readonly RoutedEvent NotificationClosingEvent = EventManager.RegisterRoutedEvent(
           "NotificationClosing", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        private TimeSpan _closingAnimationTime = TimeSpan.Zero;

        private Rectangle _colorBar = null!;

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

        public Duration ExpirationTime
        {
            get { return (Duration)GetValue(ExpirationTimeProperty); }
            set { SetValue(ExpirationTimeProperty, value); }
        }

        public bool IsClosing { get; set; }

        public async Task CloseAsync()
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
            _colorBar = (Rectangle)GetTemplateChild("PART_ColorBar");

            var closingStoryboards = Template.Triggers.OfType<EventTrigger>().FirstOrDefault(t => t.RoutedEvent == NotificationCloseInvokedEvent)?.Actions.OfType<BeginStoryboard>().Select(a => a.Storyboard);
            _closingAnimationTime = new TimeSpan(closingStoryboards?.Max(s => Math.Min((s.Duration.HasTimeSpan ? s.Duration.TimeSpan + (s.BeginTime ?? TimeSpan.Zero) : TimeSpan.MaxValue).Ticks, s.Children.Select(ch => ch.Duration.TimeSpan + (s.BeginTime ?? TimeSpan.Zero)).Max().Ticks)) ?? 0);
        }

        public virtual async Task ScheduleCloseAsync(TimeSpan expirationTime)
        {
            ExpirationTime = expirationTime;
            RaiseEvent(new RoutedEventArgs(NotificationClosingEvent));
            await Task.Delay(10);

            ShrinkColorBarOverTime(expirationTime);

            if (expirationTime == TimeSpan.MaxValue)
            {
                return;
            }
            await Task.Delay(expirationTime);

            await CloseAsync();
        }

        private void ShrinkColorBarOverTime(TimeSpan expirationTime)
        {
            if (Content is not string && Content is not NotificationContent)
            {
                _colorBar.Visibility = Visibility.Collapsed;
                return;
            }

            if (_colorBar is null)
                return;

            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                From = _colorBar.RenderSize.Width,
                To = 0,
                Duration = new Duration(expirationTime),
                EasingFunction = new QuadraticEase()
            };

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(widthAnimation);

            Storyboard.SetTarget(widthAnimation, _colorBar);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(FrameworkElement.WidthProperty));

            storyboard.Begin();
        }
    }
}