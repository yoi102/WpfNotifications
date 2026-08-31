using System;
using System.Threading;
using System.Threading.Tasks;

namespace Notifications.Internal
{
    internal sealed class NotificationExpirationTimer
    {
        private CancellationTokenSource? _delayCancellation;
        private CancellationTokenRegistration _externalCancellationRegistration;
        private TaskCompletionSource<object?>? _completion;
        private CancellationToken _externalCancellation;
        private Func<Task>? _onExpired;
        private TimeSpan? _pausedRemaining;

        public long? DeadlineUtcTicks { get; private set; }

        public Task Start(TimeSpan expirationTime, CancellationToken cancellationToken, Func<Task> onExpired)
        {
            Stop(true);
            _externalCancellation = cancellationToken;
            _onExpired = onExpired;
            DeadlineUtcTicks = CalculateDeadlineUtcTicks(expirationTime);
            _completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (cancellationToken.CanBeCanceled)
            {
                _externalCancellationRegistration = cancellationToken.Register(() => _completion?.TrySetResult(null));
            }

            StartSegment(expirationTime);
            return _completion.Task;
        }

        public void Pause()
        {
            if (!DeadlineUtcTicks.HasValue)
            {
                return;
            }

            _pausedRemaining = TimeSpan.FromTicks(Math.Max(0, DeadlineUtcTicks.Value - DateTime.UtcNow.Ticks));
            DeadlineUtcTicks = null;
            _delayCancellation?.Cancel();
        }

        public bool Resume()
        {
            if (!_pausedRemaining.HasValue)
            {
                return false;
            }

            var remaining = _pausedRemaining.Value;
            _pausedRemaining = null;
            DeadlineUtcTicks = CalculateDeadlineUtcTicks(remaining);
            StartSegment(remaining);
            return true;
        }

        public void Stop(bool completeSchedule)
        {
            DeadlineUtcTicks = null;
            _pausedRemaining = null;
            _delayCancellation?.Cancel();
            if (completeSchedule)
            {
                _completion?.TrySetResult(null);
                _completion = null;
                _onExpired = null;
                _externalCancellationRegistration.Dispose();
            }
        }

        public void Complete()
        {
            _completion?.TrySetResult(null);
            _completion = null;
            _onExpired = null;
            _externalCancellationRegistration.Dispose();
        }

        private void StartSegment(TimeSpan delay)
        {
            _delayCancellation = CancellationTokenSource.CreateLinkedTokenSource(_externalCancellation);
            _ = RunSegmentAsync(delay, _delayCancellation);
        }

        private async Task RunSegmentAsync(TimeSpan delay, CancellationTokenSource cancellation)
        {
            try
            {
                await NotificationDelay.DelayAsync(delay, cancellation.Token);
                if (_onExpired != null)
                {
                    await _onExpired();
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
            finally
            {
                if (ReferenceEquals(_delayCancellation, cancellation))
                {
                    _delayCancellation = null;
                }

                cancellation.Dispose();
            }
        }

        private static long CalculateDeadlineUtcTicks(TimeSpan expirationTime)
        {
            var now = DateTime.UtcNow.Ticks;
            return expirationTime.Ticks >= DateTime.MaxValue.Ticks - now
                ? DateTime.MaxValue.Ticks
                : now + expirationTime.Ticks;
        }
    }

    internal static class NotificationDelay
    {
        private static readonly TimeSpan MaximumDelay = TimeSpan.FromMilliseconds(int.MaxValue - 1);

        public static async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            while (delay > MaximumDelay)
            {
                await Task.Delay(MaximumDelay, cancellationToken);
                delay -= MaximumDelay;
            }

            await Task.Delay(delay, cancellationToken);
        }
    }
}
