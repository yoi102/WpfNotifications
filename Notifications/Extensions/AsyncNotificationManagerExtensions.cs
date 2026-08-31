using System;
using System.Threading;
using System.Threading.Tasks;

namespace Notifications.Extensions
{
    /// <summary>Provides concise overloads for the advanced notification API.</summary>
    public static class AsyncNotificationManagerExtensions
    {
        /// <summary>Displays content in an application notification area.</summary>
        public static Task<INotificationHandle> ShowAsync(
            this IAsyncNotificationManager manager,
            object content,
            string areaIdentifier,
            TimeSpan? expirationTime = null,
            CancellationToken cancellationToken = default)
        {
            if (manager is null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            return manager.ShowAsync(
                new NotificationRequest(content)
                {
                    Target = NotificationTarget.Area(areaIdentifier),
                    ExpirationTime = expirationTime,
                },
                cancellationToken);
        }

        /// <summary>Displays content in a desktop overlay.</summary>
        public static Task<INotificationHandle> ShowOverlayAsync(
            this IAsyncNotificationManager manager,
            object content,
            NotificationMonitor monitor = NotificationMonitor.Primary,
            TimeSpan? expirationTime = null,
            CancellationToken cancellationToken = default)
        {
            if (manager is null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            return manager.ShowAsync(
                new NotificationRequest(content)
                {
                    Target = NotificationTarget.Overlay(monitor),
                    ExpirationTime = expirationTime,
                },
                cancellationToken);
        }

        /// <summary>Clears an application notification area.</summary>
        public static Task ClearAsync(
            this IAsyncNotificationManager manager,
            string areaIdentifier,
            CancellationToken cancellationToken = default)
        {
            if (manager is null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            return manager.ClearAsync(NotificationTarget.Area(areaIdentifier), cancellationToken);
        }
    }
}
