using Notifications.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Notifications.Internal
{
    internal sealed class NotificationStore
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<Guid, NotificationHandle> _active = new Dictionary<Guid, NotificationHandle>();
        private readonly Dictionary<string, NotificationHandle> _tags = new Dictionary<string, NotificationHandle>(StringComparer.Ordinal);

        public NotificationHandle? FindByTag(string? tag)
        {
            if (tag is null)
            {
                return null;
            }

            lock (_syncRoot)
            {
                if (_tags.TryGetValue(tag, out var handle) && !handle.IsClosing)
                {
                    return handle;
                }

                _tags.Remove(tag);
                return null;
            }
        }

        public void Add(NotificationHandle handle, string? tag)
        {
            lock (_syncRoot)
            {
                _active[handle.Id] = handle;
                if (tag != null)
                {
                    _tags[tag] = handle;
                }
            }

            _ = handle.Completion.ContinueWith(
                _ => Remove(handle, tag),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        public void CloseAll(NotificationCloseReason reason)
        {
            NotificationHandle[] handles;
            lock (_syncRoot)
            {
                handles = _active.Values.ToArray();
                _tags.Clear();
            }

            foreach (var handle in handles)
            {
                handle.Close(reason);
            }
        }

        private void Remove(NotificationHandle handle, string? tag)
        {
            lock (_syncRoot)
            {
                _active.Remove(handle.Id);
                if (tag != null && _tags.TryGetValue(tag, out var current) && ReferenceEquals(current, handle))
                {
                    _tags.Remove(tag);
                }
            }
        }
    }
}
