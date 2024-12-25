using Notifications.Enums;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Notifications.Controls
{
    public class NotificationArea : Control
    {
        // Using a DependencyProperty as the backing store for Identifier.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdentifierProperty =
            DependencyProperty.Register("Identifier", typeof(string), typeof(NotificationArea), new PropertyMetadata(""));

        public static readonly DependencyProperty MaxItemsProperty =
            DependencyProperty.Register("MaxItems", typeof(uint), typeof(NotificationArea), new PropertyMetadata(uint.MaxValue));

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(NotificationPosition), typeof(NotificationArea), new PropertyMetadata(NotificationPosition.BottomRight));


        public Thickness NotificationMargin
        {
            get { return (Thickness)GetValue(NotificationMarginProperty); }
            set { SetValue(NotificationMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NotificationMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotificationMarginProperty =
            DependencyProperty.Register("NotificationMargin", typeof(Thickness), typeof(NotificationArea), new PropertyMetadata(new Thickness(8, 8, 8, 0)));


        private IList _items = null!;

        static NotificationArea()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationArea),
                new FrameworkPropertyMetadata(typeof(NotificationArea)));
        }

        public NotificationArea()
        {
            NotificationManager.AddArea(this);
        }

        public string Identifier
        {
            get { return (string)GetValue(IdentifierProperty); }
            set { SetValue(IdentifierProperty, value); }
        }

        public uint MaxItems
        {
            get { return (uint)GetValue(MaxItemsProperty); }
            set { SetValue(MaxItemsProperty, value); }
        }

        public NotificationPosition Position
        {
            get { return (NotificationPosition)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_Items") is not Panel itemsControl)
            {
                throw new ApplicationException();
            }
            _items = itemsControl.Children;
        }

        public async Task ShowAsync(object content, bool closeOnClick, TimeSpan expirationTime, Action? onClick, Action? onClose)
        {
            if (content is not Notification notification)
            {
                notification = new()
                {
                    Content = content
                };
            }
            if (notification.Style is null)
            {
                Style style = (Style)this.FindResource("NotificationBase");

                notification.Style = style;
            }
            if (closeOnClick)
            {
                notification.MouseLeftButtonDown += (sender, args) =>
                {
                    onClick?.Invoke();
                    (sender as Notification)?.CloseAsync();
                };
            }

            notification.NotificationClosed += (sender, args) => onClose?.Invoke();
            notification.NotificationClosed += OnNotificationClosed;

            if (!IsLoaded)
            {
                return;
            }

            var ownerWindow = Window.GetWindow(this);
            var x = PresentationSource.FromVisual(ownerWindow);
            if (x == null)
            {
                return;
            }

            lock (_items)
            {
                _items.Add(new ContentControl() { Content = notification, Margin = this.NotificationMargin });

                if (_items.OfType<Notification>().Count(i => !i.IsClosing) > MaxItems)
                {
                    _items.OfType<Notification>().First(i => !i.IsClosing).CloseAsync().GetAwaiter();
                }
            }

            await notification.ScheduleCloseAsync(expirationTime);
        }

        private void OnNotificationClosed(object sender, RoutedEventArgs routedEventArgs)
        {
            var notification = sender as Notification;
            _items.Remove(notification?.Parent);
        }
    }
}