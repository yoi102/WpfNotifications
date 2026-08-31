using Notifications.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Notifications
{
    internal sealed class NotificationHandle : INotificationHandle
    {
        private readonly Notification? _notification;
        private readonly Task<Enums.NotificationCloseReason>? _completed;

        public NotificationHandle(Notification notification)
        {
            _notification = notification;
        }

        private NotificationHandle()
        {
            _completed = Task.FromResult(Enums.NotificationCloseReason.Programmatic);
        }

        public Guid Id { get; } = Guid.NewGuid();

        public Task<Enums.NotificationCloseReason> Completion => _notification?.Completion ?? _completed!;

        internal static NotificationHandle CreateCompleted() => new NotificationHandle();

        internal bool IsClosing => _notification?.IsClosing ?? true;

        internal void Close(Enums.NotificationCloseReason reason)
        {
            if (_notification != null)
            {
                _ = _notification.CloseAsync(reason);
            }
        }

        internal void Update(object content, TimeSpan expirationTime)
        {
            if (_notification is null || _notification.IsClosing)
            {
                return;
            }

            _notification.Content = content;
            _ = _notification.ScheduleCloseAsync(expirationTime);
        }

        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_notification is null)
            {
                return;
            }

            Task closeTask;
            if (_notification.Dispatcher.CheckAccess())
            {
                closeTask = _notification.CloseAsync();
            }
            else
            {
                closeTask = await _notification.Dispatcher
                    .InvokeAsync(() => _notification.CloseAsync(), DispatcherPriority.Normal, cancellationToken)
                    .Task;
            }

            await closeTask;
        }

        public async Task UpdateAsync(object content, CancellationToken cancellationToken = default)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (_notification is null || _notification.IsClosing)
            {
                throw new InvalidOperationException("The notification has already closed.");
            }

            if (content is DispatcherObject dispatcherObject && dispatcherObject.Dispatcher != _notification.Dispatcher)
            {
                throw new InvalidOperationException("Notification UI content must belong to the notification's Dispatcher.");
            }

            if (_notification.Dispatcher.CheckAccess())
            {
                _notification.Content = content;
                return;
            }

            await _notification.Dispatcher.InvokeAsync(
                () => _notification.Content = content,
                DispatcherPriority.Normal,
                cancellationToken).Task;
        }
    }
}
