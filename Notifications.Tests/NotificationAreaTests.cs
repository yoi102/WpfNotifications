using Microsoft.VisualStudio.TestTools.UnitTesting;
using Notifications.Controls;
using Notifications.Enums;
using Notifications.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Notifications.Tests;

[TestClass]
[DoNotParallelize]
public class NotificationAreaTests
{
    [TestMethod]
    public Task CenterLeft_notifications_are_left_aligned()
    {
        return StaTest.RunAsync(async () =>
        {
            var (window, area, panel) = CreateLoadedArea(NotificationPosition.CenterLeft);
            try
            {
                area.Show(CreatePermanentNotification(), false, TimeSpan.MaxValue, null, null);
                await FlushDispatcherAsync();

                var container = (NotificationContainer)panel.Children[0];
                Assert.AreEqual(HorizontalAlignment.Left, container.HorizontalAlignment);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Protected_permanent_notifications_still_count_toward_MaxItems()
    {
        return StaTest.RunAsync(async () =>
        {
            var (window, area, panel) = CreateLoadedArea(NotificationPosition.BottomRight);
            try
            {
                area.MaxItems = 1;
                area.AllowRemovingPermanentOnOverflow = false;
                var permanent = CreatePermanentNotification();
                var temporary = CreatePermanentNotification();

                area.Show(permanent, false, TimeSpan.MaxValue, null, null);
                area.Show(temporary, false, TimeSpan.FromHours(1), null, null);
                await FlushDispatcherAsync();

                Assert.AreEqual(1, panel.Children.Count);
                Assert.AreSame(permanent, ((NotificationContainer)panel.Children[0]).Notification);
                Assert.AreEqual(NotificationCloseReason.Overflow, await temporary.Completion);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Direct_show_applies_explicit_display_options()
    {
        return StaTest.RunAsync(async () =>
        {
            var (window, area, panel) = CreateLoadedArea(NotificationPosition.BottomRight);
            try
            {
                var notification = CreatePermanentNotification();
                var shown = area.Show(
                    notification,
                    TimeSpan.MaxValue,
                    new NotificationDisplayOptions
                    {
                        PauseOnHover = false,
                        PauseOnKeyboardFocus = false,
                        ShowCloseButton = false,
                        ShowCountdownBar = false,
                    });
                await FlushDispatcherAsync();

                Assert.AreSame(notification, shown);
                Assert.AreEqual(1, panel.Children.Count);
                Assert.IsFalse(notification.PauseOnHover);
                Assert.IsFalse(notification.PauseOnKeyboardFocus);
                Assert.IsFalse(notification.ShowCloseButton);
                Assert.IsFalse(notification.ShowCountdownBar);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Area_registers_again_after_being_reloaded()
    {
        return StaTest.RunAsync(async () =>
        {
            var (window, area, panel) = CreateLoadedArea(NotificationPosition.BottomRight);
            var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
            try
            {
                manager.Show(CreatePermanentNotification(), area.Identifier, expirationTime: TimeSpan.MaxValue);
                Assert.AreEqual(1, panel.Children.Count);

                area.Clear();
                await FlushDispatcherAsync();

                window.Content = null;
                await FlushDispatcherAsync();
                window.Content = area;
                window.UpdateLayout();
                await FlushDispatcherAsync();

                manager.Show(CreatePermanentNotification(), area.Identifier, expirationTime: TimeSpan.MaxValue);
                panel = (Panel)area.Template.FindName("PART_Items", area);
                Assert.AreEqual(1, panel.Children.Count);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Manager_Clear_can_be_called_from_a_worker_thread()
    {
        return StaTest.RunAsync(async () =>
        {
            var (window, area, panel) = CreateLoadedArea(NotificationPosition.BottomRight);
            var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
            var identifier = area.Identifier;
            try
            {
                manager.Show(CreatePermanentNotification(), identifier, expirationTime: TimeSpan.MaxValue);
                Assert.AreEqual(1, panel.Children.Count);

                await Task.Run(() => manager.Clear(identifier));
                await FlushDispatcherAsync();

                Assert.AreEqual(0, panel.Children.Count);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Manager_Show_can_be_called_from_a_worker_thread()
    {
        return StaTest.RunAsync(async () =>
        {
            var (window, area, panel) = CreateLoadedArea(NotificationPosition.BottomRight);
            var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
            var identifier = area.Identifier;
            try
            {
                await Task.Run(() => manager.Show("background", identifier, expirationTime: TimeSpan.MaxValue));
                await FlushDispatcherAsync();

                Assert.AreEqual(1, panel.Children.Count);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Overlay_closes_after_its_last_notification()
    {
        return StaTest.RunAsync(async () =>
        {
            var closed = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var window = new NotificationsOverlayWindow
            {
                Opacity = 0,
                ShowActivated = false,
            };
            window.Closed += (_, _) => closed.TrySetResult(null);
            var notificationClosed = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var notification = CreatePermanentNotification();
            notification.NotificationClosed += (_, _) => notificationClosed.TrySetResult(null);

            window.Show();
            window.ShowNotification(
                notification,
                false,
                TimeSpan.FromMilliseconds(10),
                null,
                null,
                new NotificationManagerOptions());

            var notificationCompleted = await Task.WhenAny(notificationClosed.Task, Task.Delay(TimeSpan.FromSeconds(2)));
            Assert.AreSame(notificationClosed.Task, notificationCompleted, "The notification did not close.");

            var windowCompleted = await Task.WhenAny(closed.Task, Task.Delay(TimeSpan.FromSeconds(2)));
            Assert.AreSame(closed.Task, windowCompleted, "The overlay did not close after its last notification.");
        });
    }

    [TestMethod]
    public Task Overlay_is_compact_nonactivating_and_inside_work_area()
    {
        return StaTest.RunAsync(async () =>
        {
            var options = new NotificationManagerOptions
            {
                Overlay = new NotificationOverlayOptions
                {
                    Position = NotificationPosition.TopLeft,
                    MaxItems = 3,
                },
            };
            var workArea = SystemParameters.WorkArea;
            var window = new NotificationsOverlayWindow(options.Overlay, workArea)
            {
                Opacity = 0,
                ShowActivated = false,
            };
            try
            {
                window.Show();
                var notification = CreatePermanentNotification();
                var shown = window.ShowNotification(notification, false, TimeSpan.MaxValue, null, null, options);
                window.UpdateLayout();

                var handle = new WindowInteropHelper(window).Handle;
                var extendedStyle = handle.GetWindowLong((int)WindowExtensions.GetWindowLongFields.GWL_EXSTYLE).ToInt64();
                Assert.AreNotEqual(0L, extendedStyle & (long)WindowExtensions.ExtendedWindowStyles.WS_EX_NOACTIVATE);
                Assert.IsLessThan(workArea.Width, window.ActualWidth, "Overlay should not cover the complete work area.");
                Assert.IsLessThan(workArea.Height, window.ActualHeight, "Overlay should not cover the complete work area.");
                Assert.AreEqual(workArea.Left, window.Left, 1d);
                Assert.AreEqual(workArea.Top, window.Top, 1d);

                Assert.IsNotNull(shown);
                await shown.CloseAsync();
            }
            finally
            {
                if (!window.Dispatcher.HasShutdownStarted && window.IsLoaded)
                {
                    window.Close();
                }
            }
        });
    }

    private static (Window Window, NotificationArea Area, Panel Panel) CreateLoadedArea(NotificationPosition position)
    {
        var window = new Window
        {
            Width = 400,
            Height = 300,
            ShowActivated = false,
            ShowInTaskbar = false,
            Opacity = 0,
        };

        var area = new NotificationArea
        {
            Identifier = $"test-{Guid.NewGuid():N}",
            Position = position,
        };

        window.Content = area;
        window.Show();
        area.ApplyTemplate();
        window.UpdateLayout();

        var panel = (Panel)area.Template.FindName("PART_Items", area);
        return (window, area, panel);
    }

    private static Notification CreatePermanentNotification()
    {
        return new Notification
        {
            ClosingAnimationDuration = TimeSpan.Zero,
            Style = new Style(typeof(Notification)),
        };
    }

    private static async Task FlushDispatcherAsync()
    {
        await Dispatcher.CurrentDispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
    }
}
