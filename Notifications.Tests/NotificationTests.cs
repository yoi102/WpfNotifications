using Microsoft.VisualStudio.TestTools.UnitTesting;
using Notifications.Controls;
using Notifications.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Notifications.Tests;

[TestClass]
[DoNotParallelize]
public class NotificationTests
{
    [TestMethod]
    public Task ScheduleClose_rejects_negative_expiration()
    {
        return StaTest.RunAsync(() =>
        {
            var notification = new Notification();

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => notification.ScheduleClose(TimeSpan.FromMilliseconds(-2)));

            return Task.CompletedTask;
        });
    }

    [TestMethod]
    public Task ScheduleCloseAsync_closes_once_after_expiration()
    {
        return StaTest.RunAsync(async () =>
        {
            var closedCount = 0;
            var notification = new Notification { ClosingAnimationDuration = TimeSpan.Zero };
            notification.NotificationClosed += (_, _) => closedCount++;

            await notification.ScheduleCloseAsync(TimeSpan.FromMilliseconds(10));
            await notification.CloseAsync();

            Assert.AreEqual(1, closedCount);
            Assert.IsTrue(notification.IsClosing);
        });
    }

    [TestMethod]
    public Task Permanent_notification_does_not_schedule_a_close()
    {
        return StaTest.RunAsync(async () =>
        {
            var notification = new Notification { ClosingAnimationDuration = TimeSpan.Zero };

            await notification.ScheduleCloseAsync(TimeSpan.MaxValue);

            Assert.IsTrue(notification.IsPermanent);
            Assert.IsFalse(notification.IsClosing);
        });
    }

    [TestMethod]
    public Task Completion_reports_programmatic_close_reason()
    {
        return StaTest.RunAsync(async () =>
        {
            var notification = new Notification
            {
                AnimationsEnabled = false,
                ClosingAnimationDuration = TimeSpan.Zero,
            };

            await notification.CloseAsync();

            Assert.AreEqual(NotificationCloseReason.Programmatic, await notification.Completion);
        });
    }

    [TestMethod]
    public Task Scheduled_close_reports_expired_reason()
    {
        return StaTest.RunAsync(async () =>
        {
            var notification = new Notification
            {
                AnimationsEnabled = false,
                ClosingAnimationDuration = TimeSpan.Zero,
            };

            await notification.ScheduleCloseAsync(TimeSpan.FromMilliseconds(10));

            Assert.AreEqual(NotificationCloseReason.Expired, await notification.Completion);
        });
    }

    [TestMethod]
    public Task Scheduling_expiration_raises_new_and_compatibility_events_once()
    {
        return StaTest.RunAsync(async () =>
        {
            var expirationScheduledCount = 0;
            var compatibilityEventCount = 0;
            var notification = new Notification
            {
                AnimationsEnabled = false,
                ClosingAnimationDuration = TimeSpan.Zero,
            };
            notification.ExpirationScheduled += (_, _) => expirationScheduledCount++;
            notification.NotificationClosing += (_, _) => compatibilityEventCount++;

            await notification.ScheduleCloseAsync(TimeSpan.FromMilliseconds(10));

            Assert.AreEqual(1, expirationScheduledCount);
            Assert.AreEqual(1, compatibilityEventCount);
        });
    }

    [TestMethod]
    public Task Hidden_countdown_bar_does_not_disable_expiration()
    {
        return StaTest.RunAsync(async () =>
        {
            var notification = new Notification
            {
                AnimationsEnabled = false,
                ClosingAnimationDuration = TimeSpan.Zero,
                ShowCountdownBar = false,
            };

            await notification.ScheduleCloseAsync(TimeSpan.FromMilliseconds(10));

            Assert.AreEqual(NotificationCloseReason.Expired, await notification.Completion);
        });
    }

    [TestMethod]
    public Task Unloaded_notification_still_expires()
    {
        return StaTest.RunAsync(async () =>
        {
            var notification = new Notification
            {
                AnimationsEnabled = false,
                ClosingAnimationDuration = TimeSpan.Zero,
            };
            var host = new ContentControl { Content = notification };
            var window = new Window
            {
                Content = host,
                Opacity = 0,
                ShowActivated = false,
                ShowInTaskbar = false,
            };
            try
            {
                window.Show();
                var scheduled = notification.ScheduleCloseAsync(TimeSpan.FromMilliseconds(20));
                host.Content = null;

                await scheduled;

                Assert.AreEqual(NotificationCloseReason.Expired, await notification.Completion);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Hover_pauses_and_resumes_the_expiration_timer()
    {
        return StaTest.RunAsync(async () =>
        {
            var notification = new Notification
            {
                AnimationsEnabled = false,
                ClosingAnimationDuration = TimeSpan.Zero,
            };
            var scheduled = notification.ScheduleCloseAsync(TimeSpan.FromMilliseconds(40));
            notification.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });

            await Task.Delay(80);
            Assert.IsFalse(notification.IsClosing, "A hovered notification expired instead of pausing.");
            Assert.IsFalse(scheduled.IsCompleted, "ScheduleCloseAsync completed while the timer was paused.");

            notification.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseLeaveEvent });
            await scheduled;

            Assert.AreEqual(NotificationCloseReason.Expired, await notification.Completion);
        });
    }

    [TestMethod]
    public Task Default_template_exposes_an_accessible_close_button()
    {
        return StaTest.RunAsync(async () =>
        {
            var notification = new Notification
            {
                Content = "message",
                AnimationsEnabled = false,
                ClosingAnimationDuration = TimeSpan.Zero,
            };
            var window = new Window
            {
                Content = notification,
                Opacity = 0,
                ShowActivated = false,
                ShowInTaskbar = false,
            };
            try
            {
                window.Show();
                notification.ApplyTemplate();
                var closeButton = notification.Template.FindName("PART_CloseButton", notification) as ButtonBase;

                Assert.IsNotNull(closeButton);
                Assert.AreEqual(notification.CloseButtonAutomationName, AutomationProperties.GetName(closeButton));
                closeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.AreEqual(NotificationCloseReason.User, await notification.Completion);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Default_template_can_hide_close_button_and_countdown_bar()
    {
        return StaTest.RunAsync(() =>
        {
            var notification = new Notification
            {
                Content = "message",
                ShowCloseButton = false,
                ShowCountdownBar = false,
            };
            var window = new Window
            {
                Content = notification,
                Opacity = 0,
                ShowActivated = false,
                ShowInTaskbar = false,
            };
            try
            {
                window.Show();
                notification.ApplyTemplate();
                window.UpdateLayout();
                var closeButton = notification.Template.FindName("PART_CloseButton", notification) as ButtonBase;
                var countdownBar = notification.Template.FindName("PART_CountdownBar", notification) as Rectangle;

                Assert.IsNotNull(closeButton);
                Assert.IsNotNull(countdownBar);
                Assert.AreEqual(Visibility.Collapsed, closeButton.Visibility);
                Assert.AreEqual(Visibility.Collapsed, countdownBar.Visibility);

                notification.ShowCloseButton = true;
                notification.ShowCountdownBar = true;
                window.UpdateLayout();

                Assert.AreEqual(Visibility.Visible, closeButton.Visibility);
                Assert.AreEqual(Visibility.Visible, countdownBar.Visibility);
            }
            finally
            {
                window.Close();
            }

            return Task.CompletedTask;
        });
    }

    [TestMethod]
    public Task Cancelling_a_schedule_keeps_the_notification_open()
    {
        return StaTest.RunAsync(async () =>
        {
            var notification = new Notification
            {
                AnimationsEnabled = false,
                ClosingAnimationDuration = TimeSpan.Zero,
            };
            using var cancellation = new CancellationTokenSource();
            var scheduled = notification.ScheduleCloseAsync(TimeSpan.FromMilliseconds(30), cancellation.Token);

            cancellation.Cancel();
            await scheduled;
            await Task.Delay(60);

            Assert.IsFalse(notification.IsClosing);
            await notification.CloseAsync();
        });
    }
}
