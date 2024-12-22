using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Notifications.Controls
{
    public class NotificationArea : Control
    {


        public string Identifier
        {
            get { return (string)GetValue(IdentifierProperty); }
            set { SetValue(IdentifierProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Identifier.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdentifierProperty =
            DependencyProperty.Register("Identifier", typeof(string), typeof(NotificationArea), new PropertyMetadata(""));


        public NotificationPosition Position
        {
            get { return (NotificationPosition)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(NotificationPosition), typeof(NotificationArea), new PropertyMetadata(NotificationPosition.BottomRight));


        public uint MaxItems
        {
            get { return (uint)GetValue(MaxItemsProperty); }
            set { SetValue(MaxItemsProperty, value); }
        }

        public static readonly DependencyProperty MaxItemsProperty =
            DependencyProperty.Register("MaxItems", typeof(uint), typeof(NotificationArea), new PropertyMetadata(uint.MaxValue));

        private IList _items = null!;

        public NotificationArea()
        {
            NotificationManager.AddArea(this);
        }

        static NotificationArea()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationArea),
                new FrameworkPropertyMetadata(typeof(NotificationArea)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var itemsControl = GetTemplateChild("PART_Items") as Panel;
            if (itemsControl is null )
            {
                throw new ApplicationException();
            }
            _items = itemsControl.Children;
        }

        public async void Show(Notification content, TimeSpan expirationTime, Action? onClick, Action? onClose)
        {
            var notification = content;

            if (content.Style is null)
            {
                Style style = (Style)this.FindResource(typeof(Notification));

                content.Style = style;
            }

            notification.MouseLeftButtonDown += (sender, args) =>
            {
                if (onClick != null)
                {
                    onClick.Invoke();
                    (sender as Notification)?.CloseAsync();
                }
            };
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
                _items.Add(notification);

                if (_items.OfType<Notification>().Count(i => !i.IsClosing) > MaxItems)
                {
                    _items.OfType<Notification>().First(i => !i.IsClosing).CloseAsync();
                }
            }


            if (expirationTime == TimeSpan.MaxValue)
            {
                return;
            }
            await Task.Delay(expirationTime);

            notification.CloseAsync();

        }

        private void OnNotificationClosed(object sender, RoutedEventArgs routedEventArgs)
        {
            var notification = sender as Notification;
            _items.Remove(notification);
        }

    }

    public enum NotificationPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}