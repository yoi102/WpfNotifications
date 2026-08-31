using Notifications.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notifications.Internal
{
    internal sealed class NotificationOverlayHost
    {
        private readonly NotificationOverlayOptions _options;
        private readonly Dictionary<OverlayKey, NotificationsOverlayWindow> _windows =
            new Dictionary<OverlayKey, NotificationsOverlayWindow>();

        public NotificationOverlayHost(NotificationOverlayOptions options)
        {
            _options = options;
        }

        public NotificationsOverlayWindow GetOrCreate(NotificationTarget target)
        {
            var monitor = MonitorHelper.Resolve(target);
            var key = new OverlayKey(monitor.Handle, _options.Position);
            if (_windows.TryGetValue(key, out var existingWindow))
            {
                return existingWindow;
            }

            var window = new NotificationsOverlayWindow(_options, monitor.WorkArea);
            window.Closed += (_, _) =>
            {
                if (_windows.TryGetValue(key, out var current) && ReferenceEquals(current, window))
                {
                    _windows.Remove(key);
                }
            };
            _windows.Add(key, window);
            return window;
        }

        public void ClearAll()
        {
            foreach (var window in _windows.Values.ToArray())
            {
                window.Clear();
            }
        }

        public Task ClearAsync(NotificationTarget target)
        {
            var monitor = MonitorHelper.Resolve(target);
            var key = new OverlayKey(monitor.Handle, _options.Position);
            return _windows.TryGetValue(key, out var window) ? window.ClearAsync() : Task.CompletedTask;
        }

        public void CloseAll()
        {
            foreach (var window in _windows.Values.ToArray())
            {
                window.Close();
            }

            _windows.Clear();
        }

        private readonly struct OverlayKey : IEquatable<OverlayKey>
        {
            public OverlayKey(IntPtr monitorHandle, NotificationPosition position)
            {
                MonitorHandle = monitorHandle;
                Position = position;
            }

            private IntPtr MonitorHandle { get; }
            private NotificationPosition Position { get; }

            public bool Equals(OverlayKey other) => MonitorHandle == other.MonitorHandle && Position == other.Position;
            public override bool Equals(object? obj) => obj is OverlayKey other && Equals(other);
            public override int GetHashCode() => (MonitorHandle.GetHashCode() * 397) ^ (int)Position;
        }
    }
}
