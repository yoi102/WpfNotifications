using Microsoft.VisualStudio.TestTools.UnitTesting;
using Notifications.Controls;
using Notifications.Enums;
using Notifications.Exceptions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Notifications.Tests;

[TestClass]
[DoNotParallelize]
public class NotificationManagerTests
{
    [TestMethod]
    public Task ShowAsync_returns_handle_with_close_result()
    {
        return StaTest.RunAsync(async () =>
        {
            var identifier = $"test-{Guid.NewGuid():N}";
            var area = new NotificationArea { Identifier = identifier };
            var window = CreateWindow(area);
            try
            {
                var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
                var notification = new Notification
                {
                    AnimationsEnabled = false,
                    ClosingAnimationDuration = TimeSpan.Zero,
                    Style = new Style(typeof(Notification)),
                };
                var handle = await manager.ShowAsync(new NotificationRequest(notification)
                {
                    Target = NotificationTarget.Area(identifier),
                    CloseOnClick = false,
                    ExpirationTime = TimeSpan.MaxValue,
                });

                await handle.CloseAsync();

                Assert.AreEqual(NotificationCloseReason.Programmatic, await handle.Completion);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task ShowAsync_reports_a_missing_area()
    {
        return StaTest.RunAsync(async () =>
        {
            var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
            await Assert.ThrowsExactlyAsync<NotificationAreaNotFoundException>(async () =>
                await manager.ShowAsync(new NotificationRequest("message")
                {
                    Target = NotificationTarget.Area($"missing-{Guid.NewGuid():N}"),
                }));
        });
    }

    [TestMethod]
    public Task Duplicate_area_identifiers_are_rejected()
    {
        return StaTest.RunAsync(async () =>
        {
            var identifier = $"duplicate-{Guid.NewGuid():N}";
            var grid = new Grid();
            grid.Children.Add(new NotificationArea { Identifier = identifier });
            grid.Children.Add(new NotificationArea { Identifier = identifier });
            var window = CreateWindow(grid);
            try
            {
                var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
                await Assert.ThrowsExactlyAsync<DuplicateNotificationAreaException>(async () =>
                    await manager.ShowAsync(new NotificationRequest("message")
                    {
                        Target = NotificationTarget.Area(identifier),
                    }));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Default_theme_is_loaded_automatically()
    {
        return StaTest.RunAsync(() =>
        {
            var area = new NotificationArea();
            var window = CreateWindow(area);
            try
            {
                Assert.IsNotNull(area.Template);
                Assert.IsNotNull(area.Template.FindName("PART_Items", area));
            }
            finally
            {
                window.Close();
            }

            return Task.CompletedTask;
        });
    }

    [TestMethod]
    public Task Matching_tag_can_update_an_existing_notification()
    {
        return StaTest.RunAsync(async () =>
        {
            var identifier = $"test-{Guid.NewGuid():N}";
            var area = new NotificationArea { Identifier = identifier };
            var window = CreateWindow(area);
            try
            {
                var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
                var notification = new Notification
                {
                    AnimationsEnabled = false,
                    ClosingAnimationDuration = TimeSpan.Zero,
                    Style = new Style(typeof(Notification)),
                };
                var first = await manager.ShowAsync(new NotificationRequest(notification)
                {
                    Target = NotificationTarget.Area(identifier),
                    Tag = "download",
                    CloseOnClick = false,
                    ExpirationTime = TimeSpan.MaxValue,
                });
                var second = await manager.ShowAsync(new NotificationRequest("50%")
                {
                    Target = NotificationTarget.Area(identifier),
                    Tag = "download",
                    DuplicateBehavior = NotificationDuplicateBehavior.UpdateExisting,
                    ExpirationTime = TimeSpan.MaxValue,
                });

                Assert.AreEqual(first.Id, second.Id);
                Assert.AreEqual("50%", notification.Content);
                await first.CloseAsync();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task ClearAsync_waits_for_close_and_reports_clear_reason()
    {
        return StaTest.RunAsync(async () =>
        {
            var identifier = $"test-{Guid.NewGuid():N}";
            var area = new NotificationArea { Identifier = identifier };
            var window = CreateWindow(area);
            try
            {
                var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
                var notification = new Notification
                {
                    AnimationsEnabled = false,
                    ClosingAnimationDuration = TimeSpan.Zero,
                    Style = new Style(typeof(Notification)),
                };
                var handle = await manager.ShowAsync(new NotificationRequest(notification)
                {
                    Target = NotificationTarget.Area(identifier),
                    ExpirationTime = TimeSpan.MaxValue,
                });

                await manager.ClearAsync(NotificationTarget.Area(identifier));

                Assert.AreEqual(NotificationCloseReason.Cleared, await handle.Completion);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Advanced_requests_do_not_close_on_content_click_by_default()
    {
        return StaTest.RunAsync(() =>
        {
            Assert.IsFalse(new NotificationRequest("message").CloseOnClick);
            return Task.CompletedTask;
        });
    }

    [TestMethod]
    public Task Request_visibility_settings_override_manager_defaults()
    {
        return StaTest.RunAsync(async () =>
        {
            var identifier = $"test-{Guid.NewGuid():N}";
            var area = new NotificationArea { Identifier = identifier };
            var window = CreateWindow(area);
            try
            {
                using var manager = new NotificationManager(new NotificationManagerOptions
                {
                    ShowCloseButton = false,
                    ShowCountdownBar = false,
                }, Dispatcher.CurrentDispatcher);
                var defaultNotification = CreateTestNotification();
                var overriddenNotification = CreateTestNotification();

                var defaultHandle = await manager.ShowAsync(new NotificationRequest(defaultNotification)
                {
                    Target = NotificationTarget.Area(identifier),
                    ExpirationTime = TimeSpan.MaxValue,
                });
                var overriddenHandle = await manager.ShowAsync(new NotificationRequest(overriddenNotification)
                {
                    Target = NotificationTarget.Area(identifier),
                    ExpirationTime = TimeSpan.MaxValue,
                    ShowCloseButton = true,
                    ShowCountdownBar = true,
                });

                Assert.IsFalse(defaultNotification.ShowCloseButton);
                Assert.IsFalse(defaultNotification.ShowCountdownBar);
                Assert.IsTrue(overriddenNotification.ShowCloseButton);
                Assert.IsTrue(overriddenNotification.ShowCountdownBar);

                await defaultHandle.CloseAsync();
                await overriddenHandle.CloseAsync();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Disposing_manager_closes_its_notifications()
    {
        return StaTest.RunAsync(async () =>
        {
            var identifier = $"test-{Guid.NewGuid():N}";
            var area = new NotificationArea { Identifier = identifier };
            var window = CreateWindow(area);
            try
            {
                var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
                var notification = new Notification
                {
                    AnimationsEnabled = false,
                    ClosingAnimationDuration = TimeSpan.Zero,
                    Style = new Style(typeof(Notification)),
                };
                var handle = await manager.ShowAsync(new NotificationRequest(notification)
                {
                    Target = NotificationTarget.Area(identifier),
                    ExpirationTime = TimeSpan.MaxValue,
                });

                manager.Dispose();

                Assert.AreEqual(NotificationCloseReason.ManagerDisposed, await handle.Completion);
                Assert.ThrowsExactly<ObjectDisposedException>(() => manager.Show("again"));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Background_legacy_errors_are_observable()
    {
        return StaTest.RunAsync(async () =>
        {
            var identifier = $"duplicate-{Guid.NewGuid():N}";
            var grid = new Grid();
            grid.Children.Add(new NotificationArea { Identifier = identifier });
            grid.Children.Add(new NotificationArea { Identifier = identifier });
            var window = CreateWindow(grid);
            try
            {
                var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
                var error = new TaskCompletionSource<NotificationManagerErrorEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
                manager.Error += (_, args) => error.TrySetResult(args);

                await Task.Run(() => manager.Show("message", identifier, expirationTime: TimeSpan.MaxValue));
                var completed = await Task.WhenAny(error.Task, Task.Delay(TimeSpan.FromSeconds(2)));

                Assert.AreSame(error.Task, completed);
                Assert.AreEqual(NotificationManagerOperation.Show, error.Task.Result.Operation);
                Assert.IsInstanceOfType<DuplicateNotificationAreaException>(error.Task.Result.Exception);
                manager.Dispose();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Duplicate_tag_ignore_and_replace_behaviors_are_deterministic()
    {
        return StaTest.RunAsync(async () =>
        {
            var identifier = $"test-{Guid.NewGuid():N}";
            var area = new NotificationArea { Identifier = identifier };
            var window = CreateWindow(area);
            try
            {
                var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
                var firstNotification = CreateTestNotification();
                var first = await manager.ShowAsync(new NotificationRequest(firstNotification)
                {
                    Target = NotificationTarget.Area(identifier),
                    Tag = "job",
                    ExpirationTime = TimeSpan.MaxValue,
                });
                var ignored = await manager.ShowAsync(new NotificationRequest("ignored")
                {
                    Target = NotificationTarget.Area(identifier),
                    Tag = "job",
                    DuplicateBehavior = NotificationDuplicateBehavior.Ignore,
                    ExpirationTime = TimeSpan.MaxValue,
                });

                Assert.AreEqual(first.Id, ignored.Id);

                var replacement = await manager.ShowAsync(new NotificationRequest(CreateTestNotification())
                {
                    Target = NotificationTarget.Area(identifier),
                    Tag = "job",
                    DuplicateBehavior = NotificationDuplicateBehavior.Replace,
                    ExpirationTime = TimeSpan.MaxValue,
                });

                Assert.AreNotEqual(first.Id, replacement.Id);
                Assert.AreEqual(NotificationCloseReason.Replaced, await first.Completion);
                await replacement.CloseAsync();
                manager.Dispose();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Advanced_show_and_clear_work_from_a_worker_thread()
    {
        return StaTest.RunAsync(async () =>
        {
            var identifier = $"test-{Guid.NewGuid():N}";
            var area = new NotificationArea { Identifier = identifier };
            var window = CreateWindow(area);
            try
            {
                var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
                var notification = CreateTestNotification();
                var handle = await Task.Run(async () => await manager.ShowAsync(new NotificationRequest(notification)
                {
                    Target = NotificationTarget.Area(identifier),
                    ExpirationTime = TimeSpan.MaxValue,
                }));

                await Task.Run(async () => await manager.ClearAsync(NotificationTarget.Area(identifier)));

                Assert.AreEqual(NotificationCloseReason.Cleared, await handle.Completion);
                manager.Dispose();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [TestMethod]
    public Task Invalid_options_and_enum_values_are_rejected_early()
    {
        return StaTest.RunAsync(() =>
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new NotificationManager(new NotificationManagerOptions
            {
                Overlay = new NotificationOverlayOptions { MaxItems = 0 },
            }));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => NotificationTarget.Overlay((NotificationMonitor)999));
            Assert.ThrowsExactly<ArgumentException>(() => NotificationTarget.Overlay(NotificationMonitor.Primary, new Window()));

            return Task.CompletedTask;
        });
    }

    [TestMethod]
    public Task Closed_handles_reject_content_updates()
    {
        return StaTest.RunAsync(async () =>
        {
            var identifier = $"test-{Guid.NewGuid():N}";
            var area = new NotificationArea { Identifier = identifier };
            var window = CreateWindow(area);
            try
            {
                var manager = new NotificationManager(Dispatcher.CurrentDispatcher);
                var handle = await manager.ShowAsync(new NotificationRequest(CreateTestNotification())
                {
                    Target = NotificationTarget.Area(identifier),
                    ExpirationTime = TimeSpan.MaxValue,
                });
                await handle.CloseAsync();

                await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await handle.UpdateAsync("late"));
                manager.Dispose();
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static Window CreateWindow(object content)
    {
        var window = new Window
        {
            Width = 400,
            Height = 300,
            ShowActivated = false,
            ShowInTaskbar = false,
            Opacity = 0,
            Content = content,
        };
        window.Show();
        window.UpdateLayout();
        return window;
    }

    private static Notification CreateTestNotification()
    {
        return new Notification
        {
            AnimationsEnabled = false,
            ClosingAnimationDuration = TimeSpan.Zero,
            Style = new Style(typeof(Notification)),
        };
    }
}
