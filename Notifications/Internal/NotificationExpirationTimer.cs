using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Notifications.Internal
{
    internal sealed class NotificationExpirationTimer
    {
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        public event Action? Cancelled;
        private CancellationTokenSource? _delayCancellation;
        private CancellationTokenRegistration _externalCancellationRegistration;
        private TaskCompletionSource<object?>? _completion;
        private CancellationToken _externalCancellation;
        private Func<Task>? _onExpired;
        private TimeSpan? _pausedRemaining;

        public long? DeadlineUtcTicks { get; private set; }

        public Task Start(TimeSpan expirationTime, CancellationToken cancellationToken, Func<Task> onExpired)
        {
            _dispatcher.VerifyAccess();
            Stop(true);
            if (cancellationToken.IsCancellationRequested)
            {
                Cancelled?.Invoke();
                return Task.CompletedTask;
            }
            _externalCancellation = cancellationToken;
            _onExpired = onExpired;
            DeadlineUtcTicks = CalculateDeadlineUtcTicks(expirationTime);
            _completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var completion = _completion;
            if (cancellationToken.CanBeCanceled)
            {
                // Never synchronously wait for the Dispatcher inside a cancellation callback:
                // Stop may dispose this registration while the UI thread is rescheduling.
                _externalCancellationRegistration = cancellationToken.Register(() =>
                    _dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ReferenceEquals(_completion, completion))
                        {
                            CancelSchedule();
                        }
                    })));
            }

            StartSegment(expirationTime);
            return completion.Task;
        }

        public void Pause()
        {
            _dispatcher.VerifyAccess();
            if (!DeadlineUtcTicks.HasValue)
            {
                return;
            }

            _pausedRemaining = TimeSpan.FromTicks(Math.Max(0, DeadlineUtcTicks.Value - DateTime.UtcNow.Ticks));
            DeadlineUtcTicks = null;
            CancelSegment();
        }

        public bool Resume()
        {
            _dispatcher.VerifyAccess();
            if (_externalCancellation.IsCancellationRequested)
            {
                CancelSchedule();
                return false;
            }
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
            _dispatcher.VerifyAccess();
            DeadlineUtcTicks = null;
            _pausedRemaining = null;
            CancelSegment();
            if (completeSchedule)
            {
                var completion = _completion;
                _completion = null;
                _onExpired = null;
                _externalCancellationRegistration.Dispose();
                _externalCancellationRegistration = default;
                _externalCancellation = default;
                completion?.TrySetResult(null);
            }
        }

        public void Complete()
        {
            Stop(true);
        }

        private void CancelSchedule()
        {
            Stop(true);
            Cancelled?.Invoke();
        }

        private void CancelSegment()
        {
            var cancellation = _delayCancellation;
            _delayCancellation = null;
            cancellation?.Cancel();
        }

        private void StartSegment(TimeSpan delay)
        {
            _delayCancellation = CancellationTokenSource.CreateLinkedTokenSource(_externalCancellation);
            _ = RunSegmentAsync(delay, _delayCancellation);
        }

        private async Task RunSegmentAsync(TimeSpan delay, CancellationTokenSource cancellation)
        {
            var completion = _completion;
            try
            {
                // An STA caller can own this Dispatcher without having installed
                // a synchronization context yet (for example during app startup).
                if (SynchronizationContext.Current is not DispatcherSynchronizationContext)
                {
                    await Dispatcher.Yield(DispatcherPriority.Normal);
                }
                await NotificationDelay.DelayAsync(delay, cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();
                if (ReferenceEquals(_delayCancellation, cancellation) && _onExpired != null)
                {
                    await _onExpired();
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                completion?.TrySetException(exception);
                if (ReferenceEquals(_completion, completion))
                {
                    Stop(true);
                }
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
