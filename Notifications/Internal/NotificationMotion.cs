using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Notifications.Internal
{
    // One visual-only animation owner, independent of the notification's ControlTemplate/style.
    internal sealed class NotificationMotion : IDisposable
    {
        private readonly FrameworkElement target;
        private readonly Transform originalTransform;
        private readonly double originalOpacity;
        private readonly TranslateTransform offset = new TranslateTransform();
        private TaskCompletionSource<object?>? closing;
        private bool disposed;

        internal NotificationMotion(FrameworkElement target, bool hidden)
        {
            this.target = target;
            originalTransform = target.RenderTransform;
            originalOpacity = target.Opacity;
            var transform = new TransformGroup();
            transform.Children.Add(originalTransform);
            transform.Children.Add(offset);
            target.SetCurrentValue(UIElement.RenderTransformProperty, transform);
            if (hidden) SetOpacity(0);
            target.Unloaded += OnUnloaded;
        }

        internal void Enter(TimeSpan duration, double distance, bool animated)
        {
            if (disposed || closing != null) return;
            if (!animated || duration == TimeSpan.Zero)
            {
                target.BeginAnimation(UIElement.OpacityProperty, null);
                offset.BeginAnimation(TranslateTransform.YProperty, null);
                return;
            }
            target.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, originalOpacity, duration)
            { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }, FillBehavior = FillBehavior.Stop });
            offset.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(distance, 0, duration)
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }, FillBehavior = FillBehavior.Stop });
        }

        internal Task ExitAsync(TimeSpan duration, double distance, bool animated)
        {
            if (closing != null) return closing.Task;
            closing = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (disposed || !animated || duration == TimeSpan.Zero || !target.IsVisible)
            {
                CompleteExit();
                return closing.Task;
            }
            // Start from the currently rendered values even when the user closes during entry.
            double opacity = target.Opacity, position = offset.Y;
            var fade = new DoubleAnimation(opacity, 0, duration)
            { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
            fade.Completed += (_, _) => CompleteExit();
            target.BeginAnimation(UIElement.OpacityProperty, fade);
            offset.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(position, distance, duration)
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } });
            return closing.Task;
        }

        private void CompleteExit()
        {
            if (!disposed) SetOpacity(0);
            offset.BeginAnimation(TranslateTransform.YProperty, null);
            closing?.TrySetResult(null);
        }
        private void SetOpacity(double value)
        {
            // Seek immediately: a zero-duration BeginAnimation alone waits for the next render tick.
            // That can leave a deferred card visible for a frame or complete a disabled exit at opacity 1.
            var clock = (AnimationClock)new DoubleAnimation(value, value, TimeSpan.Zero).CreateClock(true);
            target.ApplyAnimationClock(UIElement.OpacityProperty, clock);
            clock.Controller!.SeekAlignedToLastTick(TimeSpan.Zero, TimeSeekOrigin.BeginTime);
        }
        private void OnUnloaded(object sender, RoutedEventArgs args)
        {
            // A removed host cannot deliver more visible frames; don't strand CloseAsync.
            if (closing != null) CompleteExit();
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            target.Unloaded -= OnUnloaded;
            target.BeginAnimation(UIElement.OpacityProperty, null);
            offset.BeginAnimation(TranslateTransform.YProperty, null);
            target.SetCurrentValue(UIElement.RenderTransformProperty, originalTransform);
            closing?.TrySetResult(null);
        }
    }
}
