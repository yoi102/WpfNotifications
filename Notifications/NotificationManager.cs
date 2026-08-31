using Notifications.Constants;
using Notifications.Controls;
using Notifications.Enums;
using Notifications.Exceptions;
using Notifications.Internal;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Notifications
{
    /// <summary>Displays and manages application-area and desktop-overlay notifications.</summary>
    public class NotificationManager : IAsyncNotificationManager, IDisposable
    {
        private readonly Dispatcher _dispatcher;
        private readonly NotificationManagerOptions _options;
        private readonly NotificationOverlayHost _overlayHost;
        private readonly NotificationStore _notifications = new NotificationStore();
        private int _disposed;

        /// <summary>Initializes a manager using legacy process-wide defaults.</summary>
        public NotificationManager(Dispatcher? dispatcher = null)
            : this(NotificationManagerOptions.FromLegacyDefaults(), dispatcher)
        {
        }

        /// <summary>Initializes a manager using an immutable snapshot of the supplied options.</summary>
        public NotificationManager(NotificationManagerOptions options, Dispatcher? dispatcher = null)
        {
            _options = (options ?? throw new ArgumentNullException(nameof(options))).Clone();
            _dispatcher = dispatcher ?? Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            _overlayHost = new NotificationOverlayHost(_options.Overlay);
            NotificationConstants.ApplyConfiguredApplicationResources();
        }

        /// <summary>Raised when a background legacy <see cref="Show"/> or <see cref="Clear"/> operation fails.</summary>
        public event EventHandler<NotificationManagerErrorEventArgs>? Error;

        /// <summary>Clears overlay notifications or notifications in the named application area.</summary>
        public void Clear(string areaIdentifier = "")
        {
            ThrowIfDisposed();
            if (areaIdentifier is null)
            {
                throw new ArgumentNullException(nameof(areaIdentifier));
            }

            if (!_dispatcher.CheckAccess())
            {
                DispatchLegacy(NotificationManagerOperation.Clear, () => ClearCore(areaIdentifier));
                return;
            }

            ClearCore(areaIdentifier);
        }

        /// <summary>Clears a target and completes after its closing animations finish.</summary>
        public Task ClearAsync(NotificationTarget target, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            cancellationToken.ThrowIfCancellationRequested();
            return _dispatcher.CheckAccess()
                ? ClearCoreAsync(target, true)
                : ClearOnDispatcherAsync(target, cancellationToken);
        }

        /// <summary>Displays a notification through the source-compatible legacy API.</summary>
        public void Show(object content, string areaIdentifier = "", bool closeOnClick = true, TimeSpan? expirationTime = null, Action? onClick = null, Action? onClose = null)
        {
            ThrowIfDisposed();
            ValidateContent(content);
            if (areaIdentifier is null)
            {
                throw new ArgumentNullException(nameof(areaIdentifier));
            }

            var effectiveExpirationTime = expirationTime ?? _options.DefaultExpirationTime;
            Notification.ValidateExpirationTime(effectiveExpirationTime, nameof(expirationTime));
            var request = new NotificationRequest(content)
            {
                Target = areaIdentifier.Length == 0 ? NotificationTarget.Overlay() : NotificationTarget.Area(areaIdentifier),
                CloseOnClick = closeOnClick,
                ExpirationTime = effectiveExpirationTime,
                OnClick = onClick,
                OnClose = onClose,
            };

            if (!_dispatcher.CheckAccess())
            {
                DispatchLegacy(NotificationManagerOperation.Show, () => ShowCore(request, false));
                return;
            }

            _ = ShowCore(request, false);
        }

        /// <summary>Displays a request and returns a handle for the individual notification.</summary>
        public Task<INotificationHandle> ShowAsync(NotificationRequest request, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ValidateRequest(request);
            cancellationToken.ThrowIfCancellationRequested();
            return _dispatcher.CheckAccess()
                ? Task.FromResult<INotificationHandle>(ShowCore(request, true))
                : ShowOnDispatcherAsync(request, cancellationToken);
        }

        /// <summary>Releases overlay windows and closes notifications created by this manager.</summary>
        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            if (_dispatcher.CheckAccess())
            {
                DisposeCore();
            }
            else if (!_dispatcher.HasShutdownStarted && !_dispatcher.HasShutdownFinished)
            {
                _dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        DisposeCore();
                    }
                    catch (Exception exception)
                    {
                        ReportError(NotificationManagerOperation.Dispose, exception);
                    }
                }));
            }

            GC.SuppressFinalize(this);
        }

        private void ClearCore(string areaIdentifier)
        {
            if (areaIdentifier.Length == 0)
            {
                _overlayHost.ClearAll();
                return;
            }

            foreach (var area in NotificationAreaRegistry.Find(_dispatcher, areaIdentifier))
            {
                area.Clear();
            }
        }

        private async Task<INotificationHandle> ShowOnDispatcherAsync(NotificationRequest request, CancellationToken cancellationToken)
        {
            return await _dispatcher.InvokeAsync(
                () => (INotificationHandle)ShowCore(request, true),
                DispatcherPriority.Normal,
                cancellationToken).Task;
        }

        private NotificationHandle ShowCore(NotificationRequest request, bool requireTarget)
        {
            ThrowIfDisposed();
            ValidateRequest(request);
            var expirationTime = request.ExpirationTime ?? _options.DefaultExpirationTime;
            Notification.ValidateExpirationTime(expirationTime, nameof(request.ExpirationTime));
            var displayOptions = NotificationDisplayOptions.FromManager(_options, request);

            var existing = _notifications.FindByTag(request.Tag);
            if (existing != null)
            {
                switch (request.DuplicateBehavior)
                {
                    case NotificationDuplicateBehavior.Ignore:
                        return existing;
                    case NotificationDuplicateBehavior.UpdateExisting:
                        existing.Update(request.Content, expirationTime);
                        return existing;
                    case NotificationDuplicateBehavior.Replace:
                        existing.Close(NotificationCloseReason.Replaced);
                        break;
                }
            }

            Notification? notification;
            if (request.Target.IsOverlay)
            {
                var window = _overlayHost.GetOrCreate(request.Target);
                if (!window.IsVisible)
                {
                    window.Show();
                }

                notification = window.ShowNotification(
                    request.Content,
                    request.CloseOnClick,
                    expirationTime,
                    request.OnClick,
                    request.OnClose,
                    displayOptions);
            }
            else
            {
                var identifier = request.Target.AreaIdentifier!;
                var area = GetSingleArea(identifier, requireTarget);
                if (area is null)
                {
                    return NotificationHandle.CreateCompleted();
                }

                notification = area.ShowManaged(
                    request.Content,
                    request.CloseOnClick,
                    expirationTime,
                    request.OnClick,
                    request.OnClose,
                    displayOptions);
            }

            if (notification is null)
            {
                if (requireTarget)
                {
                    throw new NotificationAreaNotFoundException(request.Target.AreaIdentifier ?? string.Empty);
                }

                return NotificationHandle.CreateCompleted();
            }

            var handle = new NotificationHandle(notification);
            _notifications.Add(handle, request.Tag);
            return handle;
        }

        private async Task ClearOnDispatcherAsync(NotificationTarget target, CancellationToken cancellationToken)
        {
            var clearTask = await _dispatcher.InvokeAsync(
                () => ClearCoreAsync(target, true),
                DispatcherPriority.Normal,
                cancellationToken).Task;
            await clearTask;
        }

        private Task ClearCoreAsync(NotificationTarget target, bool requireTarget)
        {
            ThrowIfDisposed();
            if (target.IsOverlay)
            {
                return _overlayHost.ClearAsync(target);
            }

            var area = GetSingleArea(target.AreaIdentifier!, requireTarget);
            return area?.ClearAsync(NotificationCloseReason.Cleared) ?? Task.CompletedTask;
        }

        private NotificationArea? GetSingleArea(string identifier, bool required)
        {
            var areas = NotificationAreaRegistry.Find(_dispatcher, identifier);
            if (areas.Length == 0)
            {
                if (required)
                {
                    throw new NotificationAreaNotFoundException(identifier);
                }

                return null;
            }

            if (areas.Length > 1)
            {
                throw new DuplicateNotificationAreaException(identifier);
            }

            return areas[0];
        }

        private void ValidateContent(object content)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (content is DispatcherObject dispatcherObject && dispatcherObject.Dispatcher != _dispatcher)
            {
                throw new InvalidOperationException("Notification UI content must belong to the notification manager's Dispatcher.");
            }
        }

        private void ValidateRequest(NotificationRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ValidateContent(request.Content);
            if (request.Target is null)
            {
                throw new ArgumentException("A notification target is required.", nameof(request));
            }

            if (request.Target.Owner != null && request.Target.Owner.Dispatcher != _dispatcher)
            {
                throw new InvalidOperationException("The owner window must belong to the notification manager's Dispatcher.");
            }

            if (request.Tag != null && string.IsNullOrWhiteSpace(request.Tag))
            {
                throw new ArgumentException("A notification tag cannot be empty or whitespace.", nameof(request));
            }

            if (!Enum.IsDefined(typeof(NotificationDuplicateBehavior), request.DuplicateBehavior))
            {
                throw new ArgumentOutOfRangeException(nameof(request), request.DuplicateBehavior, "Unknown duplicate behavior.");
            }
        }

        private void DispatchLegacy(NotificationManagerOperation operation, Action action)
        {
            _dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    ReportError(operation, exception);
                }
            }));
        }

        private void DisposeCore()
        {
            _notifications.CloseAll(NotificationCloseReason.ManagerDisposed);
            _overlayHost.CloseAll();
        }

        private void ReportError(NotificationManagerOperation operation, Exception exception)
        {
            Trace.TraceError($"Notification manager {operation} failed: {exception}");
            Error?.Invoke(this, new NotificationManagerErrorEventArgs(operation, exception));
        }

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                throw new ObjectDisposedException(nameof(NotificationManager));
            }
        }
    }
}
