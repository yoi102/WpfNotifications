using Notifications.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace Notifications.Internal
{
    internal static class NotificationAreaRegistry
    {
        private static readonly object SyncRoot = new object();
        private static readonly List<WeakReference<NotificationArea>> Areas = new List<WeakReference<NotificationArea>>();

        public static void Register(NotificationArea area)
        {
            lock (SyncRoot)
            {
                RemoveDeadAreas();
                if (!Areas.Any(reference => reference.TryGetTarget(out var target) && ReferenceEquals(target, area)))
                {
                    Areas.Add(new WeakReference<NotificationArea>(area));
                }
            }
        }

        public static void Unregister(NotificationArea area)
        {
            lock (SyncRoot)
            {
                Areas.RemoveAll(reference => !reference.TryGetTarget(out var target) || ReferenceEquals(target, area));
            }
        }

        public static NotificationArea[] Find(Dispatcher dispatcher, string identifier)
        {
            lock (SyncRoot)
            {
                RemoveDeadAreas();
                return Areas
                    .Select(reference => reference.TryGetTarget(out var area) ? area : null)
                    .Where(area => area != null && area.Dispatcher == dispatcher && area.IsLoaded && area.Identifier == identifier)
                    .Cast<NotificationArea>()
                    .ToArray();
            }
        }

        private static void RemoveDeadAreas()
        {
            Areas.RemoveAll(reference => !reference.TryGetTarget(out _));
        }
    }
}
