using Notifications.Enums;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Notifications.Controls
{
    public class NotificationArea : Control
    {
        // Using a DependencyProperty as the backing store for AllowRemovingPermanentOnOverflow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowRemovingPermanentOnOverflowProperty =
            DependencyProperty.Register("AllowRemovingPermanentOnOverflow", typeof(bool), typeof(NotificationArea), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for Identifier.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdentifierProperty =
            DependencyProperty.Register("Identifier", typeof(string), typeof(NotificationArea), new PropertyMetadata(""));

        public static readonly DependencyProperty MaxItemsProperty =
            DependencyProperty.Register("MaxItems", typeof(uint), typeof(NotificationArea), new PropertyMetadata(uint.MaxValue));

        // Using a DependencyProperty as the backing store for NotificationMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotificationMarginProperty =
            DependencyProperty.Register("NotificationMargin", typeof(Thickness), typeof(NotificationArea), new PropertyMetadata(new Thickness(8, 8, 8, 0)));

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(NotificationPosition), typeof(NotificationArea), new PropertyMetadata(NotificationPosition.BottomRight));

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

        public bool AllowRemovingPermanentOnOverflow
        {
            get { return (bool)GetValue(AllowRemovingPermanentOnOverflowProperty); }
            set { SetValue(AllowRemovingPermanentOnOverflowProperty, value); }
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

        public Thickness NotificationMargin
        {
            get { return (Thickness)GetValue(NotificationMarginProperty); }
            set { SetValue(NotificationMarginProperty, value); }
        }

        public NotificationPosition Position
        {
            get { return (NotificationPosition)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public void Clear()
        {
            foreach (var item in _items.OfType<NotificationContainer>().ToArray())
            {
                item.Notification.Close();
            }
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

        public void Show(object content, bool closeOnClick, TimeSpan expirationTime, Action? onClick, Action? onClose)
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
                    (sender as Notification)?.Close();
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
                _items.Add(new NotificationContainer(notification) { Margin = this.NotificationMargin });

                if (AllowRemovingPermanentOnOverflow)
                {
                    if (_items.OfType<NotificationContainer>().Count(i => !i.IsClosing) > MaxItems)
                    {
                        _items.OfType<NotificationContainer>().First(i => !i.IsClosing).Notification.Close();
                    };
                }
                else
                {
                    var removableNotifications = _items.OfType<NotificationContainer>().Where(n => !n.IsPermanent && !n.IsClosing);

                    if (removableNotifications.Count() > MaxItems)
                    {
                        removableNotifications.First().Notification.Close();
                    };
                }
            }

            notification.ScheduleClose(expirationTime);
        }

        private void OnNotificationClosed(object sender, RoutedEventArgs routedEventArgs)
        {
            var notification = sender as Notification;
            _items.Remove(notification?.Parent);
        }
    }
}