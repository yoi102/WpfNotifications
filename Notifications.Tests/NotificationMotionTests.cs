using Microsoft.VisualStudio.TestTools.UnitTesting;
using Notifications.Controls;
using Notifications.Internal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace Notifications.Tests;

[TestClass]
[DoNotParallelize]
public class NotificationMotionTests
{
    [TestMethod]
    public Task Custom_template_without_storyboards_uses_deferred_motion() => StaTest.RunAsync(async () =>
    {
        var card = CustomCard();
        var window = Open(card);
        try
        {
            var root = (Border)card.Template.FindName("PART_AnimationRoot", card);
            Assert.AreEqual(0d, root.Opacity, "Deferred cards must not flash before host positioning.");
            var size = card.RenderSize;
            card.PlayEntranceAnimation();
            if (SystemParameters.ClientAreaAnimation)
            {
                await Task.Delay(100);
                Assert.IsTrue(root.Opacity > 0 && root.Opacity < 1);
                Assert.IsGreaterThan(0d, root.RenderTransform.Value.OffsetY);
            }
            await Task.Delay(450);
            Assert.AreEqual(1d, root.Opacity);
            Assert.AreEqual(0d, root.RenderTransform.Value.OffsetY);
            Assert.AreEqual(size, card.RenderSize);
            Assert.IsTrue(card.LayoutTransform.Value.IsIdentity);
            card.PlayEntranceAnimation();
            Assert.AreEqual(1d, root.Opacity, "Re-showing must not restart entry.");
            var closing = card.CloseAsync();
            if (SystemParameters.ClientAreaAnimation)
            {
                await Task.Delay(70);
                Assert.IsFalse(closing.IsCompleted);
                Assert.IsTrue(root.Opacity > 0 && root.Opacity < 1);
                Assert.AreEqual(size, card.RenderSize);
            }
            await WithTimeout(closing);
            Assert.AreEqual(0d, root.Opacity);
        }
        finally { window.Close(); }
    });

    [TestMethod]
    public Task Exit_during_entry_starts_at_current_frame_and_completes_after_fade() => StaTest.RunAsync(async () =>
    {
        var root = new Border { Width = 240, Height = 80, Background = Brushes.CornflowerBlue };
        var window = Open(root);
        try
        {
            using var motion = new NotificationMotion(root, true);
            motion.Enter(TimeSpan.FromSeconds(1), 24, true);
            await Task.Delay(140);
            var opacity = root.Opacity;
            var y = root.RenderTransform.Value.OffsetY;
            Assert.IsTrue(opacity > 0 && opacity < 1);
            var close = motion.ExitAsync(TimeSpan.FromMilliseconds(260), 24, true);
            Assert.AreSame(close, motion.ExitAsync(TimeSpan.Zero, 0, false));
            Assert.AreEqual(opacity, root.Opacity, .025, "Close must not flash fully opaque.");
            Assert.AreEqual(y, root.RenderTransform.Value.OffsetY, .5);
            await Task.Delay(80);
            Assert.IsFalse(close.IsCompleted);
            Assert.IsTrue(root.Opacity < opacity && root.Opacity > 0);
            await WithTimeout(close);
            Assert.AreEqual(0d, root.Opacity);
        }
        finally { window.Close(); }
    });

    [TestMethod]
    public Task Unloading_host_during_exit_does_not_strand_close() => StaTest.RunAsync(async () =>
    {
        var card = CustomCard();
        card.ClosingAnimationDuration = TimeSpan.FromMinutes(1);
        var window = Open(card);
        card.PlayEntranceAnimation();
        var closing = card.CloseAsync();
        window.Close();
        await WithTimeout(closing);
        Assert.IsTrue(card.Completion.IsCompleted);
    });

    [TestMethod]
    public Task Disabled_and_zero_duration_motion_restore_final_opacity_immediately() => StaTest.RunAsync(async () =>
    {
        foreach (var enabled in new[] { false, true })
        {
            var card = CustomCard();
            card.AnimationsEnabled = enabled;
            card.OpeningAnimationDuration = TimeSpan.Zero;
            card.ClosingAnimationDuration = TimeSpan.Zero;
            var window = Open(card);
            try
            {
                card.PlayEntranceAnimation();
                var root = (Border)card.Template.FindName("PART_AnimationRoot", card);
                Assert.AreEqual(1d, root.Opacity);
                Assert.AreEqual(0d, root.RenderTransform.Value.OffsetY);
                Assert.IsFalse(root.HasAnimatedProperties);
                await WithTimeout(card.CloseAsync());
                Assert.AreEqual(0d, root.Opacity);
            }
            finally { window.Close(); }
        }
    });

    [TestMethod]
    public Task Existing_frozen_transform_and_opacity_are_preserved() => StaTest.RunAsync(async () =>
    {
        var transform = new ScaleTransform(1.1, 1.2);
        transform.Freeze();
        var root = new Border { Width = 200, Height = 80, RenderTransform = transform, Opacity = .7 };
        var window = Open(root);
        try
        {
            using (var motion = new NotificationMotion(root, true))
            {
                motion.Enter(TimeSpan.FromMilliseconds(80), 24, true);
                await Task.Delay(160);
                Assert.AreEqual(.7, root.Opacity);
                Assert.AreEqual(1.1, root.RenderTransform.Value.M11);
                Assert.AreEqual(1.2, root.RenderTransform.Value.M22);
            }
            Assert.AreSame(transform, root.RenderTransform);
            Assert.AreEqual(.7, root.Opacity);
        }
        finally { window.Close(); }
    });

    [TestMethod]
    public Task Legacy_template_without_animation_root_and_default_template_both_work() => StaTest.RunAsync(async () =>
    {
        foreach (var custom in new[] { true, false })
        {
            var card = custom ? CustomCard(false) : new Notification { Content = "Default style" };
            card.AnimationsEnabled = false;
            card.DeferEntranceAnimation = false;
            var window = Open(card);
            try
            {
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                Assert.AreEqual(1d, card.Opacity);
                Assert.IsTrue(card.LayoutTransform.Value.IsIdentity);
                if (!custom) Assert.IsNotNull(card.Template.FindName("PART_AnimationRoot", card));
                await WithTimeout(card.CloseAsync());
            }
            finally { window.Close(); }
        }
    });

    [TestMethod]
    public Task Close_before_reveal_never_plays_entry() => StaTest.RunAsync(async () =>
    {
        var card = CustomCard();
        var window = Open(card);
        try
        {
            var root = (Border)card.Template.FindName("PART_AnimationRoot", card);
            var close = card.CloseAsync();
            card.PlayEntranceAnimation();
            Assert.AreEqual(0d, root.Opacity);
            await WithTimeout(close);
            Assert.AreEqual(0d, root.Opacity);
        }
        finally { window.Close(); }
    });

    [TestMethod]
    public Task Invalid_motion_values_are_rejected() => StaTest.RunAsync(() =>
    {
        var card = new Notification();
        foreach (var distance in new[] { -1d, double.NaN, double.PositiveInfinity })
            Assert.ThrowsExactly<ArgumentException>(() => card.AnimationDistance = distance);
        Assert.ThrowsExactly<ArgumentException>(() => card.OpeningAnimationDuration = TimeSpan.FromTicks(-1));
        Assert.ThrowsExactly<ArgumentException>(() => card.ClosingAnimationDuration = TimeSpan.FromTicks(-1));
        return Task.CompletedTask;
    });

    private static Notification CustomCard(bool namedRoot = true) => new Notification
    {
        Width = 240, Height = 80, DeferEntranceAnimation = true,
        Style = new Style(typeof(Notification)),
        Template = (ControlTemplate)XamlReader.Parse(
            "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
            "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>" +
            "<Border " + (namedRoot ? "x:Name='PART_AnimationRoot' " : "") +
            "Background='CornflowerBlue' CornerRadius='12'/></ControlTemplate>")
    };

    private static Window Open(FrameworkElement content)
    {
        var window = new Window { Content = content, Width = 360, Height = 180,
            ShowActivated = false, ShowInTaskbar = false, Opacity = 0 };
        window.Show();
        window.UpdateLayout();
        return window;
    }

    private static async Task WithTimeout(Task task)
    {
        Assert.AreSame(task, await Task.WhenAny(task, Task.Delay(3000)), "Animation completion timed out.");
        await task;
    }
}
