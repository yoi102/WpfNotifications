using Notifications.Constants;
using Notifications.Enums;
using Notifications.Internal;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Notifications.Controls
{
    /// <summary>Hosts and positions notifications inside an application window.</summary>
    public class NotificationArea : Control
    {
        /// <summary>Identifies the <see cref="AllowRemovingPermanentOnOverflow"/> dependency property.</summary>
        public static readonly DependencyProperty AllowRemovingPermanentOnOverflowProperty =
            DependencyProperty.Register(nameof(AllowRemovingPermanentOnOverflow), typeof(bool), typeof(NotificationArea), new PropertyMetadata(true));

        /// <summary>Identifies the <see cref="ClearOnUnload"/> dependency property.</summary>
        public static readonly DependencyProperty ClearOnUnloadProperty =
            DependencyProperty.Register(nameof(ClearOnUnload), typeof(bool), typeof(NotificationArea), new PropertyMetadata(false));

        /// <summary>Identifies the <see cref="Identifier"/> dependency property.</summary>
        public static readonly DependencyProperty IdentifierProperty =
            DependencyProperty.Register(
                nameof(Identifier),
                typeof(string),
                typeof(NotificationArea),
                new PropertyMetadata(string.Empty),
                value => value != null);

        /// <summary>Identifies the <see cref="MaxItems"/> dependency property.</summary>
        public static readonly DependencyProperty MaxItemsProperty =
            DependencyProperty.Register(nameof(MaxItems), typeof(uint), typeof(NotificationArea), new PropertyMetadata(uint.MaxValue));

        /// <summary>Identifies the <see cref="NotificationMargin"/> dependency property.</summary>
        public static readonly DependencyProperty NotificationMarginProperty =
            DependencyProperty.Register(nameof(NotificationMargin), typeof(Thickness), typeof(NotificationArea), new PropertyMetadata(new Thickness(8, 8, 8, 0)));

        /// <summary>Identifies the <see cref="Position"/> dependency property.</summary>
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(NotificationPosition), typeof(NotificationArea), new PropertyMetadata(NotificationPosition.BottomRight));

        /// <summary>Identifies the <see cref="ReverseOrder"/> dependency property.</summary>
        public static readonly DependencyProperty ReverseOrderProperty =
            ReversibleStackPanel.ReverseOrderProperty.AddOwner(typeof(NotificationArea), new PropertyMetadata(false));

        private IList? _items;

        static NotificationArea()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NotificationArea),
                new FrameworkPropertyMetadata(typeof(NotificationArea)));
        }

        /// <summary>Initializes a notification area.</summary>
        public NotificationArea()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        internal event EventHandler? ItemsChanged;

        /// <summary>Gets or sets whether unloading starts closing hosted notifications.
        /// Defaults to false so temporary reparenting or navigation can preserve notifications.</summary>
        public bool ClearOnUnload
        {
            get => (bool)GetValue(ClearOnUnloadProperty);
            set => SetValue(ClearOnUnloadProperty, value);
        }

        /// <summary>Gets or sets whether overflow may evict permanent notifications.</summary>
        public bool AllowRemovingPermanentOnOverflow
        {
            get => (bool)GetValue(AllowRemovingPermanentOnOverflowProperty);
            set => SetValue(AllowRemovingPermanentOnOverflowProperty, value);
        }

        /// <summary>Gets or sets the unique manager-visible area identifier.</summary>
        public string Identifier
        {
            get => (string)GetValue(IdentifierProperty);
            set => SetValue(IdentifierProperty, value);
        }

        /// <summary>Gets or sets the maximum number of active notifications.</summary>
        public uint MaxItems
        {
            get => (uint)GetValue(MaxItemsProperty);
            set => SetValue(MaxItemsProperty, value);
        }

        /// <summary>Gets or sets the margin applied to each notification.</summary>
        public Thickness NotificationMargin
        {
            get => (Thickness)GetValue(NotificationMarginProperty);
            set => SetValue(NotificationMarginProperty, value);
        }

        /// <summary>Gets or sets the notification anchor position.</summary>
        public NotificationPosition Position
        {
            get => (NotificationPosition)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        /// <summary>Gets or sets whether notifications are arranged in reverse order.</summary>
        public bool ReverseOrder
        {
            get => (bool)GetValue(ReverseOrderProperty);
            set => SetValue(ReverseOrderProperty, value);
        }

        internal int NotificationCount => _items?.OfType<NotificationContainer>().Count() ?? 0;

        /// <summary>Starts closing all notifications in this area.</summary>
        public void Clear()
        {
            _ = ClearAsync(NotificationCloseReason.Cleared);
        }

        internal Task ClearAsync(NotificationCloseReason closeReason)
        {
            VerifyAccess();
            if (_items is null)
            {
                return Task.CompletedTask;
            }

            var notifications = _items.OfType<NotificationContainer>()
                .Select(item => item.Notification)
                .ToArray();
            var closeTasks = notifications
                .Select(notification => notification.CloseAsync(closeReason))
                .ToArray();
            return Task.WhenAll(closeTasks);
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Items") is not Panel itemsPanel)
            {
                throw new InvalidOperationException("The NotificationArea template must contain a Panel named PART_Items.");
            }

            if (_items != null && !ReferenceEquals(_items, itemsPanel.Children))
            {
                var existingItems = _items.OfType<UIElement>().ToArray();
                _items.Clear();
                foreach (var item in existingItems)
                {
                    itemsPanel.Children.Add(item);
                }
            }
            _items = itemsPanel.Children;
            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Displays content directly in this area.</summary>
        public void Show(
            object content,
            bool closeOnClick,
            TimeSpan expirationTime,
            Action? onClick,
            Action? onClose)
        {
            _ = ShowManaged(
                content,
                closeOnClick,
                expirationTime,
                onClick,
                onClose,
                new NotificationDisplayOptions());
        }

        /// <summary>Displays content directly in this area using explicit presentation options.</summary>
        /// <returns>The displayed control, or <see langword="null"/> when this area is not loaded.</returns>
        public Notification? Show(
            object content,
            TimeSpan expirationTime,
            NotificationDisplayOptions displayOptions,
            bool closeOnClick = false,
            Action? onClick = null,
            Action? onClose = null)
        {
            if (displayOptions is null)
            {
                throw new ArgumentNullException(nameof(displayOptions));
            }

            return ShowManaged(
                content,
                closeOnClick,
                expirationTime,
                onClick,
                onClose,
                displayOptions);
        }

        internal Notification? ShowManaged(
            object content,
            bool closeOnClick,
            TimeSpan expirationTime,
            Action? onClick,
            Action? onClose,
            NotificationDisplayOptions displayOptions)
        {
            VerifyAccess();
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Notification.ValidateExpirationTime(expirationTime, nameof(expirationTime));

            if (!IsLoaded || _items is null)
            {
                return null;
            }

            var notification = CreateNotification(content);
            if (notification.Parent != null)
            {
                throw new InvalidOperationException("A Notification instance that already has a visual parent cannot be shown again.");
            }
            if (notification.IsClosing || notification.Completion.IsCompleted)
            {
                throw new InvalidOperationException("A closed Notification instance cannot be shown again.");
            }

            displayOptions.ApplyTo(notification);
            if (notification.Style is null)
            {
                notification.Style = (Style)FindResource(typeof(Notification));
            }

            notification.Closed += OnNotificationClosed;
            if (onClose != null)
            {
                notification.NotificationClosed += (_, _) => onClose();
            }

            if (closeOnClick)
            {
                notification.MouseLeftButtonUp += (_, _) =>
                {
                    if (notification.IsClosing)
                    {
                        return;
                    }

                    try
                    {
                        onClick?.Invoke();
                    }
                    finally
                    {
                        _ = notification.CloseAsync(NotificationCloseReason.User);
                    }
                };
            }

            var notificationContainer = new NotificationContainer(notification)
            {
                Margin = NotificationMargin,
                HorizontalAlignment = GetAlignmentForPosition(),
            };

            _items.Add(notificationContainer);
            RemoveOverflowNotification();
            ItemsChanged?.Invoke(this, EventArgs.Empty);
            try
            {
                notification.ScheduleClose(expirationTime);
            }
            catch
            {
                // A custom scheduling override or application event may reject the show.
                // Close also releases the area subscription and any started timer.
                _ = notification.CloseAsync();
                throw;
            }
            return notification;
        }

        private HorizontalAlignment GetAlignmentForPosition()
        {
            switch (Position)
            {
                case NotificationPosition.TopLeft:
                case NotificationPosition.BottomLeft:
                case NotificationPosition.CenterLeft:
                    return HorizontalAlignment.Left;

                case NotificationPosition.TopCenter:
                case NotificationPosition.BottomCenter:
                case NotificationPosition.Center:
                    return HorizontalAlignment.Center;

                default:
                    return HorizontalAlignment.Right;
            }
        }

        private static Notification CreateNotification(object content)
        {
            if (content is Notification notification)
            {
                return notification;
            }

            notification = new Notification { Content = content };
            if (content is not UIElement)
            {
                notification.Width = NotificationConstants.NotificationWidth;
            }

            return notification;
        }

        private void OnNotificationClosed(object? sender, EventArgs eventArgs)
        {
            if (sender is Notification closedNotification)
            {
                closedNotification.Closed -= OnNotificationClosed;
            }
            if (_items != null && sender is Notification notification && notification.Parent is NotificationContainer container)
            {
                _items.Remove(container);
                ItemsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void RemoveOverflowNotification()
        {
            if (_items is null)
            {
                return;
            }

            var activeNotifications = _items
                .OfType<NotificationContainer>()
                .Where(item => !item.IsClosing)
                .ToArray();

            if ((ulong)activeNotifications.Length <= MaxItems)
            {
                return;
            }

            var notificationToRemove = AllowRemovingPermanentOnOverflow
                ? activeNotifications.FirstOrDefault()
                : activeNotifications.FirstOrDefault(item => !item.IsPermanent);

            if (notificationToRemove != null)
            {
                _ = notificationToRemove.Notification.CloseAsync(NotificationCloseReason.Overflow);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NotificationAreaRegistry.Register(this);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            NotificationAreaRegistry.Unregister(this);
            if (ClearOnUnload)
            {
                Clear();
            }
        }
    }
}
