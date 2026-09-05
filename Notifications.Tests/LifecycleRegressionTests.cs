using Microsoft.VisualStudio.TestTools.UnitTesting;
using Notifications.Controls;
using Notifications.Enums;
using Notifications.Internal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Notifications.Tests;

[TestClass]
[DoNotParallelize]
public class LifecycleRegressionTests
{
    [TestMethod]
    public Task Cancellation_clears_paused_schedule_before_task_completes() => StaTest.RunAsync(async () =>
    {
        var timer = new NotificationExpirationTimer();
        using var cancellation = new CancellationTokenSource();
        var expired = false;
        var scheduled = timer.Start(TimeSpan.FromHours(1), cancellation.Token, () =>
        {
            expired = true;
            return Task.CompletedTask;
        });
        timer.Pause();
        await Task.Run(() => cancellation.Cancel());
        await scheduled;
        Assert.IsNull(timer.DeadlineUtcTicks);
        Assert.IsFalse(timer.Resume());
        Assert.IsFalse(expired);
    });

    [TestMethod]
    public Task Queued_cancellation_cannot_cancel_a_replacement_schedule() => StaTest.RunAsync(async () =>
    {
        var timer = new NotificationExpirationTimer();
        using var cancellation = new CancellationTokenSource();
        var previous = timer.Start(TimeSpan.FromHours(1), cancellation.Token, () => Task.CompletedTask);
        cancellation.Cancel();
        var replacement = timer.Start(TimeSpan.FromHours(1), CancellationToken.None, () => Task.CompletedTask);
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        Assert.IsTrue(previous.IsCompleted);
        Assert.IsFalse(replacement.IsCompleted);
        Assert.IsNotNull(timer.DeadlineUtcTicks);
        timer.Stop(true);
        await replacement;
    });

    [TestMethod]
    public Task Already_cancelled_schedule_has_no_deadline() => StaTest.RunAsync(async () =>
    {
        var timer = new NotificationExpirationTimer();
        await timer.Start(TimeSpan.Zero, new CancellationToken(true), () => throw new AssertFailedException());
        Assert.IsNull(timer.DeadlineUtcTicks);
        Assert.IsFalse(timer.Resume());
    });

    [TestMethod]
    public Task Zero_expiration_can_complete_synchronously() => StaTest.RunAsync(async () =>
    {
        var notification = CreateNotification();
        await notification.ScheduleCloseAsync(TimeSpan.Zero);
        Assert.AreEqual(NotificationCloseReason.Expired, await notification.Completion);
    });

    [TestMethod]
    public Task Closing_event_can_reenter_CloseAsync_without_restarting_close() => StaTest.RunAsync(async () =>
    {
        var notification = CreateNotification();
        Task? reentrant = null;
        var invoked = 0;
        notification.NotificationCloseInvoked += (_, _) =>
        {
            invoked++;
            if (invoked == 1) reentrant = notification.CloseAsync();
        };
        var close = notification.CloseAsync();
        await close;
        Assert.AreSame(close, reentrant);
        Assert.AreEqual(1, invoked);
    });

    [TestMethod]
    public Task Throwing_public_close_event_cannot_prevent_area_cleanup() => StaTest.RunAsync(async () =>
    {
        var area = new NotificationArea();
        var window = Open(area);
        try
        {
            var notification = CreateNotification();
            // Subscribe before the area, which used to let this exception skip its handler.
            notification.NotificationClosed += (_, _) => throw new InvalidOperationException("application callback");
            area.Show(notification, false, TimeSpan.MaxValue, null, null);
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => notification.CloseAsync());
            Assert.AreEqual(0, area.NotificationCount);
            Assert.IsTrue(notification.Completion.IsCompleted);
        }
        finally { window.Close(); }
    });

    [TestMethod]
    public Task Throwing_close_invoked_event_still_finishes_lifetime() => StaTest.RunAsync(async () =>
    {
        var area = new NotificationArea();
        var window = Open(area);
        try
        {
            var notification = CreateNotification();
            notification.NotificationCloseInvoked += (_, _) => throw new InvalidOperationException("application callback");
            area.Show(notification, false, TimeSpan.MaxValue, null, null);
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => notification.CloseAsync());
            Assert.AreEqual(0, area.NotificationCount);
            Assert.IsTrue(notification.Completion.IsCompleted);
        }
        finally { window.Close(); }
    });

    [TestMethod]
    public Task Worker_update_rechecks_state_after_dispatch() => StaTest.RunAsync(async () =>
    {
        var notification = CreateNotification();
        notification.Content = "original";
        var handle = new NotificationHandle(notification);
        // Wait only for the worker to enqueue its update while this UI callback owns
        // the Dispatcher, then close before the queued mutation is allowed to execute.
        var queued = Task.Run(() => new[] { handle.UpdateAsync("late") }).GetAwaiter().GetResult();
        await notification.CloseAsync();
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => queued[0]);
        Assert.AreEqual("original", notification.Content);
    });

    [TestMethod]
    public Task Failed_overlay_show_releases_new_window() => StaTest.RunAsync(() =>
    {
        var notification = CreateNotification();
        var parent = new ContentControl { Content = notification };
        var host = new NotificationOverlayHost(new NotificationOverlayOptions());
        try
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => host.Show(
                new NotificationRequest(notification), TimeSpan.MaxValue, new NotificationDisplayOptions()));
            Assert.AreEqual(0, host.WindowCount);
            Assert.AreSame(parent, notification.Parent);
        }
        finally { host.CloseAll(); }
        return Task.CompletedTask;
    });

    [TestMethod]
    public Task Failed_replacement_keeps_existing_notification() => StaTest.RunAsync(async () =>
    {
        var area = new NotificationArea { Identifier = Guid.NewGuid().ToString() };
        var window = Open(area);
        using var manager = new NotificationManager();
        try
        {
            var handle = await manager.ShowAsync(new NotificationRequest(CreateNotification())
            {
                Target = NotificationTarget.Area(area.Identifier), Tag = "job", ExpirationTime = TimeSpan.MaxValue,
            });
            await Assert.ThrowsExactlyAsync<Notifications.Exceptions.NotificationAreaNotFoundException>(() =>
                manager.ShowAsync(new NotificationRequest("replacement")
                {
                    Target = NotificationTarget.Area("missing"), Tag = "job", DuplicateBehavior = NotificationDuplicateBehavior.Replace,
                }));
            Assert.IsFalse(handle.Completion.IsCompleted);
            Assert.AreEqual(1, area.NotificationCount);
            await handle.CloseAsync();
        }
        finally { window.Close(); }
    });

    [TestMethod]
    public Task Template_selection_respects_each_resource_scope() => StaTest.RunAsync(() =>
    {
        var selector = new NotificationTemplateSelector();
        var first = new ContentControl();
        var second = new ContentControl();
        var firstTemplate = new DataTemplate();
        var secondTemplate = new DataTemplate();
        first.Resources["DefaultStringTemplate"] = firstTemplate;
        second.Resources["DefaultStringTemplate"] = secondTemplate;
        Assert.AreSame(firstTemplate, selector.SelectTemplate("first", first));
        Assert.AreSame(secondTemplate, selector.SelectTemplate("second", second));
        first.Resources["DefaultStringTemplate"] = secondTemplate;
        Assert.AreSame(secondTemplate, selector.SelectTemplate("changed theme", first));
        return Task.CompletedTask;
    });

    [TestMethod]
    public Task Opt_in_unload_cleanup_closes_permanent_notifications() => StaTest.RunAsync(async () =>
    {
        var area = new NotificationArea { ClearOnUnload = true };
        var window = Open(area);
        try
        {
            var notification = CreateNotification();
            area.Show(notification, false, TimeSpan.MaxValue, null, null);
            window.Content = null;
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Assert.IsTrue(notification.Completion.IsCompleted);
            Assert.AreEqual(0, area.NotificationCount);
            Assert.AreEqual(NotificationCloseReason.Cleared, await notification.Completion);
        }
        finally { window.Close(); }
    });

    [TestMethod]
    public Task Worker_disposal_waits_for_animations_and_is_repeatable() => StaTest.RunAsync(async () =>
    {
        var area = new NotificationArea { Identifier = Guid.NewGuid().ToString() };
        var window = Open(area);
        var manager = new NotificationManager();
        try
        {
            var notification = CreateNotification();
            notification.AnimationsEnabled = true;
            notification.ClosingAnimationDuration = TimeSpan.FromMilliseconds(50);
            var handle = await manager.ShowAsync(new NotificationRequest(notification)
            {
                Target = NotificationTarget.Area(area.Identifier), ExpirationTime = TimeSpan.MaxValue,
            });
            await Task.Run(() => manager.DisposeAsync());
            Assert.IsTrue(handle.Completion.IsCompleted);
            Assert.AreEqual(NotificationCloseReason.ManagerDisposed, await handle.Completion);
            Assert.AreEqual(0, area.NotificationCount);
            Assert.AreSame(manager.DisposeAsync(), manager.DisposeAsync());
            await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => manager.ShowAsync(new NotificationRequest("late")));
        }
        finally { await manager.DisposeAsync(); window.Close(); }
    });

    [TestMethod]
    public Task Disposal_finishes_other_notifications_when_one_callback_throws() => StaTest.RunAsync(async () =>
    {
        var area = new NotificationArea { Identifier = Guid.NewGuid().ToString() };
        var window = Open(area);
        var manager = new NotificationManager();
        try
        {
            var first = CreateNotification();
            first.NotificationClosed += (_, _) => throw new InvalidOperationException("callback");
            var second = CreateNotification();
            foreach (var notification in new[] { first, second })
            {
                await manager.ShowAsync(new NotificationRequest(notification)
                {
                    Target = NotificationTarget.Area(area.Identifier), ExpirationTime = TimeSpan.MaxValue,
                });
            }
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => manager.DisposeAsync());
            Assert.IsTrue(first.Completion.IsCompleted);
            Assert.IsTrue(second.Completion.IsCompleted);
            Assert.AreEqual(0, area.NotificationCount);
        }
        finally { window.Close(); }
    });

    [TestMethod]
    public Task Retemplating_an_area_preserves_active_notifications() => StaTest.RunAsync(async () =>
    {
        var area = new NotificationArea();
        var window = Open(area);
        try
        {
            var notification = CreateNotification();
            area.Show(notification, false, TimeSpan.MaxValue, null, null);
            area.Template = (ControlTemplate)System.Windows.Markup.XamlReader.Parse(
                "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><StackPanel Name='PART_Items'/></ControlTemplate>");
            area.ApplyTemplate();
            window.UpdateLayout();
            var panel = (Panel)area.Template.FindName("PART_Items", area);
            Assert.AreEqual(1, panel.Children.Count);
            Assert.AreSame(notification, ((NotificationContainer)panel.Children[0]).Notification);
            await notification.CloseAsync();
            Assert.AreEqual(0, panel.Children.Count);
        }
        finally { window.Close(); }
    });

    [TestMethod]
    public Task Failed_show_keeps_an_existing_overlay_usable() => StaTest.RunAsync(async () =>
    {
        var host = new NotificationOverlayHost(new NotificationOverlayOptions());
        var first = CreateNotification();
        try
        {
            host.Show(new NotificationRequest(first), TimeSpan.MaxValue, new NotificationDisplayOptions());
            var invalid = CreateNotification();
            var parent = new ContentControl { Content = invalid };
            Assert.ThrowsExactly<InvalidOperationException>(() => host.Show(
                new NotificationRequest(invalid), TimeSpan.MaxValue, new NotificationDisplayOptions()));
            Assert.AreEqual(1, host.WindowCount);
            Assert.IsFalse(first.IsClosing);
            Assert.AreSame(parent, invalid.Parent);
            await first.CloseAsync();
            Assert.AreEqual(0, host.WindowCount);
        }
        finally { host.CloseAll(); }
    });

    [TestMethod]
    public Task Scheduling_without_an_installed_context_still_closes_on_UI_thread() => StaTest.RunAsync(async () =>
    {
        var notification = CreateNotification();
        var dispatcher = notification.Dispatcher;
        var closedOnUI = false;
        notification.NotificationClosed += (_, _) => closedOnUI = dispatcher.CheckAccess();
        var previous = SynchronizationContext.Current;
        Task scheduled;
        try
        {
            SynchronizationContext.SetSynchronizationContext(null);
            scheduled = notification.ScheduleCloseAsync(TimeSpan.FromMilliseconds(10));
        }
        finally { SynchronizationContext.SetSynchronizationContext(previous); }
        await scheduled;
        Assert.IsTrue(closedOnUI);
    });

    [TestMethod]
    public Task Closing_without_an_installed_context_still_cleans_up_on_UI_thread() => StaTest.RunAsync(async () =>
    {
        var area = new NotificationArea();
        var window = Open(area);
        try
        {
            var notification = CreateNotification();
            notification.AnimationsEnabled = true;
            notification.ClosingAnimationDuration = TimeSpan.FromMilliseconds(10);
            area.Show(notification, false, TimeSpan.MaxValue, null, null);
            var previous = SynchronizationContext.Current;
            Task close;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                close = notification.CloseAsync();
            }
            finally { SynchronizationContext.SetSynchronizationContext(previous); }
            await close;
            Assert.AreEqual(0, area.NotificationCount);
        }
        finally { window.Close(); }
    });

    private static Notification CreateNotification() => new Notification
    {
        AnimationsEnabled = false, ClosingAnimationDuration = TimeSpan.Zero, Style = new Style(typeof(Notification)),
    };

    private static Window Open(NotificationArea area)
    {
        var window = new Window { Content = area, Opacity = 0, ShowActivated = false, ShowInTaskbar = false, Width = 400, Height = 300 };
        window.Show();
        window.UpdateLayout();
        return window;
    }
}
