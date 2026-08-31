using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Notifications.Enums;
using Notifications.Internal;

namespace Notifications.Controls
{
    /// <summary>Displays notification content and manages its close lifecycle.</summary>
    [TemplatePart(Name = "PART_CountdownBar", Type = typeof(Rectangle))]
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(ButtonBase))]
    public class Notification : ContentControl
    {
        /// <summary>Identifies the <see cref="CountdownBarFill"/> dependency property.</summary>
        public static readonly DependencyProperty CountdownBarFillProperty =
            DependencyProperty.Register(nameof(CountdownBarFill), typeof(Brush), typeof(Notification), new PropertyMetadata(Brushes.White));

        /// <summary>Identifies the <see cref="ExpirationTime"/> dependency property.</summary>
        public static readonly DependencyProperty ExpirationTimeProperty =
            DependencyProperty.Register(nameof(ExpirationTime), typeof(Duration), typeof(Notification), new PropertyMetadata(new Duration(TimeSpan.FromSeconds(1))));

        /// <summary>Identifies the <see cref="IsPermanent"/> dependency property.</summary>
        public static readonly DependencyProperty IsPermanentProperty =
            DependencyProperty.Register(nameof(IsPermanent), typeof(bool), typeof(Notification), new PropertyMetadata(false));

        /// <summary>Identifies the <see cref="ClosingAnimationDuration"/> dependency property.</summary>
        public static readonly DependencyProperty ClosingAnimationDurationProperty =
            DependencyProperty.Register(
                nameof(ClosingAnimationDuration),
                typeof(TimeSpan),
                typeof(Notification),
                new PropertyMetadata(TimeSpan.FromMilliseconds(400)),
                value => (TimeSpan)value >= TimeSpan.Zero);

        /// <summary>Identifies the <see cref="PauseOnHover"/> dependency property.</summary>
        public static readonly DependencyProperty PauseOnHoverProperty =
            DependencyProperty.Register(nameof(PauseOnHover), typeof(bool), typeof(Notification), new PropertyMetadata(true, OnPauseOptionChanged));

        /// <summary>Identifies the <see cref="PauseOnKeyboardFocus"/> dependency property.</summary>
        public static readonly DependencyProperty PauseOnKeyboardFocusProperty =
            DependencyProperty.Register(nameof(PauseOnKeyboardFocus), typeof(bool), typeof(Notification), new PropertyMetadata(true, OnPauseOptionChanged));

        /// <summary>Identifies the <see cref="ShowCloseButton"/> dependency property.</summary>
        public static readonly DependencyProperty ShowCloseButtonProperty =
            DependencyProperty.Register(nameof(ShowCloseButton), typeof(bool), typeof(Notification), new PropertyMetadata(true));

        /// <summary>Identifies the <see cref="ShowCountdownBar"/> dependency property.</summary>
        public static readonly DependencyProperty ShowCountdownBarProperty =
            DependencyProperty.Register(
                nameof(ShowCountdownBar),
                typeof(bool),
                typeof(Notification),
                new PropertyMetadata(true, OnShowCountdownBarChanged));

        /// <summary>Identifies the <see cref="AnimationsEnabled"/> dependency property.</summary>
        public static readonly DependencyProperty AnimationsEnabledProperty =
            DependencyProperty.Register(nameof(AnimationsEnabled), typeof(bool), typeof(Notification), new PropertyMetadata(true));

        /// <summary>Identifies the <see cref="CloseButtonAutomationName"/> dependency property.</summary>
        public static readonly DependencyProperty CloseButtonAutomationNameProperty =
            DependencyProperty.Register(
                nameof(CloseButtonAutomationName),
                typeof(string),
                typeof(Notification),
                new PropertyMetadata("Close notification"),
                value => value != null);

        /// <summary>Identifies the <see cref="NotificationClosed"/> routed event.</summary>
        public static readonly RoutedEvent NotificationClosedEvent = EventManager.RegisterRoutedEvent(
            nameof(NotificationClosed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        /// <summary>Identifies the <see cref="NotificationCloseInvoked"/> routed event.</summary>
        public static readonly RoutedEvent NotificationCloseInvokedEvent = EventManager.RegisterRoutedEvent(
            nameof(NotificationCloseInvoked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        /// <summary>Identifies the <see cref="NotificationClosing"/> routed event.</summary>
        public static readonly RoutedEvent NotificationClosingEvent = EventManager.RegisterRoutedEvent(
            nameof(NotificationClosing), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        /// <summary>Identifies the <see cref="ExpirationScheduled"/> routed event.</summary>
        public static readonly RoutedEvent ExpirationScheduledEvent = EventManager.RegisterRoutedEvent(
            nameof(ExpirationScheduled), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Notification));

        private Rectangle? _countdownBar;
        private ButtonBase? _closeButton;
        private readonly NotificationExpirationTimer _expirationTimer = new NotificationExpirationTimer();
        private Task? _closeTask;
        private bool _isPointerOver;
        private bool _hasKeyboardFocus;
        private NotificationCloseReason _closeReason = NotificationCloseReason.Programmatic;
        private readonly TaskCompletionSource<NotificationCloseReason> _completion =
            new TaskCompletionSource<NotificationCloseReason>(TaskCreationOptions.RunContinuationsAsynchronously);

        static Notification()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Notification),
                new FrameworkPropertyMetadata(typeof(Notification)));
        }

        /// <summary>Initializes a notification control.</summary>
        public Notification()
        {
            AnimationsEnabled = SystemParameters.ClientAreaAnimation;
#if !NET47
            AutomationProperties.SetLiveSetting(this, AutomationLiveSetting.Polite);
#endif
            Loaded += OnNotificationLoaded;
            MouseEnter += (_, _) =>
            {
                _isPointerOver = true;
                UpdatePauseState();
            };
            MouseLeave += (_, _) =>
            {
                _isPointerOver = false;
                UpdatePauseState();
            };
            GotKeyboardFocus += (_, _) =>
            {
                _hasKeyboardFocus = true;
                UpdatePauseState();
            };
            LostKeyboardFocus += (_, _) =>
            {
                _hasKeyboardFocus = false;
                UpdatePauseState();
            };
        }

        /// <summary>Occurs after the close animation and before the control is removed.</summary>
        public event RoutedEventHandler NotificationClosed
        {
            add => AddHandler(NotificationClosedEvent, value);
            remove => RemoveHandler(NotificationClosedEvent, value);
        }

        /// <summary>Occurs when notification closing begins.</summary>
        public event RoutedEventHandler NotificationCloseInvoked
        {
            add => AddHandler(NotificationCloseInvokedEvent, value);
            remove => RemoveHandler(NotificationCloseInvokedEvent, value);
        }

        /// <summary>Occurs when an expiration schedule is configured. Retained for source compatibility; prefer <see cref="ExpirationScheduled"/>.</summary>
        public event RoutedEventHandler NotificationClosing
        {
            add => AddHandler(NotificationClosingEvent, value);
            remove => RemoveHandler(NotificationClosingEvent, value);
        }

        /// <summary>Occurs when an expiration schedule is configured.</summary>
        public event RoutedEventHandler ExpirationScheduled
        {
            add => AddHandler(ExpirationScheduledEvent, value);
            remove => RemoveHandler(ExpirationScheduledEvent, value);
        }

        /// <summary>Gets or sets the countdown indicator brush.</summary>
        public Brush CountdownBarFill
        {
            get => (Brush)GetValue(CountdownBarFillProperty);
            set => SetValue(CountdownBarFillProperty, value);
        }

        /// <summary>Gets or sets the displayed expiration duration.</summary>
        public Duration ExpirationTime
        {
            get => (Duration)GetValue(ExpirationTimeProperty);
            set => SetValue(ExpirationTimeProperty, value);
        }

        /// <summary>Gets or sets how long closing animation completion is awaited.</summary>
        public TimeSpan ClosingAnimationDuration
        {
            get => (TimeSpan)GetValue(ClosingAnimationDurationProperty);
            set => SetValue(ClosingAnimationDurationProperty, value);
        }

        /// <summary>Gets or sets whether this notification is closing.</summary>
        public bool IsClosing { get; set; }

        /// <summary>Gets or sets whether the notification has no automatic expiration.</summary>
        public bool IsPermanent
        {
            get => (bool)GetValue(IsPermanentProperty);
            set => SetValue(IsPermanentProperty, value);
        }

        /// <summary>Gets or sets whether pointer hover pauses expiration.</summary>
        public bool PauseOnHover
        {
            get => (bool)GetValue(PauseOnHoverProperty);
            set => SetValue(PauseOnHoverProperty, value);
        }

        /// <summary>Gets or sets whether keyboard focus pauses expiration.</summary>
        public bool PauseOnKeyboardFocus
        {
            get => (bool)GetValue(PauseOnKeyboardFocusProperty);
            set => SetValue(PauseOnKeyboardFocusProperty, value);
        }

        /// <summary>Gets or sets whether the default template shows its close button.</summary>
        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        /// <summary>Gets or sets whether the default template shows its expiration countdown bar.</summary>
        public bool ShowCountdownBar
        {
            get => (bool)GetValue(ShowCountdownBarProperty);
            set => SetValue(ShowCountdownBarProperty, value);
        }

        /// <summary>Gets or sets whether entry and closing animations are enabled.</summary>
        public bool AnimationsEnabled
        {
            get => (bool)GetValue(AnimationsEnabledProperty);
            set => SetValue(AnimationsEnabledProperty, value);
        }

        /// <summary>Gets or sets the localized automation name of the close button.</summary>
        public string CloseButtonAutomationName
        {
            get => (string)GetValue(CloseButtonAutomationNameProperty);
            set => SetValue(CloseButtonAutomationNameProperty, value);
        }

        internal Task<NotificationCloseReason> Completion => _completion.Task;

        /// <summary>Starts closing without waiting for completion.</summary>
        public void Close()
        {
            _ = CloseAsync();
        }

        /// <summary>Closes and completes after the closing animation.</summary>
        public Task CloseAsync()
        {
            return CloseAsync(NotificationCloseReason.Programmatic);
        }

        internal Task CloseAsync(NotificationCloseReason closeReason)
        {
            VerifyAccess();
            if (_closeTask != null)
            {
                return _closeTask;
            }

            IsClosing = true;
            _closeReason = closeReason;
            _expirationTimer.Stop(false);
            _countdownBar?.BeginAnimation(WidthProperty, null);
            RaiseEvent(new RoutedEventArgs(NotificationCloseInvokedEvent));
            _closeTask = CompleteCloseAsync();
            return _closeTask;
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            if (_countdownBar != null)
            {
                _countdownBar.Loaded -= OnCountdownBarLoaded;
            }

            if (_closeButton != null)
            {
                _closeButton.Click -= OnCloseButtonClick;
            }

            base.OnApplyTemplate();
            _countdownBar = GetTemplateChild("PART_CountdownBar") as Rectangle;
            if (_countdownBar != null)
            {
                _countdownBar.Loaded += OnCountdownBarLoaded;
                BeginCountdownAnimation();
            }

            _closeButton = GetTemplateChild("PART_CloseButton") as ButtonBase;
            if (_closeButton != null)
            {
                _closeButton.Click += OnCloseButtonClick;
            }
        }

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && !IsClosing)
            {
                e.Handled = true;
                _ = CloseAsync(NotificationCloseReason.User);
                return;
            }

            base.OnKeyDown(e);
        }

        /// <summary>Schedules automatic closing without waiting for completion.</summary>
        public virtual void ScheduleClose(TimeSpan expirationTime)
        {
            _ = ScheduleCloseAsync(expirationTime);
        }

        /// <summary>Schedules automatic closing and completes after expiration and closing.</summary>
        public virtual Task ScheduleCloseAsync(TimeSpan expirationTime, CancellationToken cancellationToken = default)
        {
            VerifyAccess();
            ValidateExpirationTime(expirationTime, nameof(expirationTime));

            if (IsClosing)
            {
                return _closeTask ?? Task.CompletedTask;
            }

            _expirationTimer.Stop(true);
            ExpirationTime = expirationTime;
            IsPermanent = expirationTime == TimeSpan.MaxValue;
            RaiseEvent(new RoutedEventArgs(ExpirationScheduledEvent));
            RaiseEvent(new RoutedEventArgs(NotificationClosingEvent));

            if (IsPermanent)
            {
                return Task.CompletedTask;
            }

            var scheduledClose = _expirationTimer.Start(
                expirationTime,
                cancellationToken,
                () => CloseAsync(NotificationCloseReason.Expired));
            BeginCountdownAnimation();
            UpdatePauseState();
            return scheduledClose;
        }

        internal static void ValidateExpirationTime(TimeSpan expirationTime, string parameterName)
        {
            if (expirationTime != TimeSpan.MaxValue && expirationTime < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(parameterName, expirationTime, "Expiration time cannot be negative.");
            }
        }

        private async Task CompleteCloseAsync()
        {
            try
            {
                await NotificationDelay.DelayAsync(AnimationsEnabled ? ClosingAnimationDuration : TimeSpan.Zero, CancellationToken.None);
                RaiseEvent(new RoutedEventArgs(NotificationClosedEvent));
            }
            finally
            {
                _completion.TrySetResult(_closeReason);
                _expirationTimer.Complete();
            }
        }

        private void BeginCountdownAnimation()
        {
            if (!ShowCountdownBar || _countdownBar is null || !_countdownBar.IsLoaded || !_expirationTimer.DeadlineUtcTicks.HasValue || IsClosing)
            {
                return;
            }

            var remainingTicks = Math.Max(0, _expirationTimer.DeadlineUtcTicks.Value - DateTime.UtcNow.Ticks);
            var remaining = TimeSpan.FromTicks(remainingTicks);
            _countdownBar.Height = 5;
            _countdownBar.BeginAnimation(
                WidthProperty,
                new DoubleAnimation
                {
                    From = _countdownBar.ActualWidth,
                    To = 0,
                    Duration = new Duration(remaining),
                    EasingFunction = new QuadraticEase(),
                });
        }

        private void PauseScheduledClose()
        {
            if (!_expirationTimer.DeadlineUtcTicks.HasValue || IsClosing)
            {
                return;
            }

            _expirationTimer.Pause();
            _countdownBar?.BeginAnimation(WidthProperty, null);
        }

        private void ResumeScheduledClose()
        {
            if (IsClosing || !_expirationTimer.Resume())
            {
                return;
            }

            BeginCountdownAnimation();
        }

        private void UpdatePauseState()
        {
            if ((PauseOnHover && _isPointerOver) || (PauseOnKeyboardFocus && _hasKeyboardFocus))
            {
                PauseScheduledClose();
            }
            else
            {
                ResumeScheduledClose();
            }
        }

        private static void OnPauseOptionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((Notification)dependencyObject).UpdatePauseState();
        }

        private static void OnShowCountdownBarChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var notification = (Notification)dependencyObject;
            if (!(bool)eventArgs.NewValue)
            {
                notification._countdownBar?.BeginAnimation(WidthProperty, null);
                return;
            }

            notification.Dispatcher.BeginInvoke(new Action(notification.BeginCountdownAnimation));
        }

        private void OnCountdownBarLoaded(object sender, RoutedEventArgs e)
        {
            BeginCountdownAnimation();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            _ = CloseAsync(NotificationCloseReason.User);
        }

        private void OnNotificationLoaded(object sender, RoutedEventArgs e)
        {
            if (!AnimationsEnabled)
            {
                BeginAnimation(OpacityProperty, null);
                Opacity = 1;
                if (LayoutTransform is ScaleTransform scale)
                {
                    if (scale.IsFrozen)
                    {
                        scale = scale.Clone();
                        LayoutTransform = scale;
                    }

                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                    scale.ScaleX = 1;
                    scale.ScaleY = 1;
                }
            }

#if !NET47
            var peer = UIElementAutomationPeer.CreatePeerForElement(this) ?? new FrameworkElementAutomationPeer(this);
            peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
#endif
        }
    }
}
