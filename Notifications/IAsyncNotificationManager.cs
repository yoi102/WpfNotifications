using System.Threading;
using System.Threading.Tasks;

namespace Notifications
{
    /// <summary>Adds observable, target-explicit operations to the legacy notification manager API.</summary>
    public interface IAsyncNotificationManager : INotificationManager
    {
        /// <summary>Displays a request and returns a handle for the resulting notification.</summary>
        Task<INotificationHandle> ShowAsync(NotificationRequest request, CancellationToken cancellationToken = default);

        /// <summary>Clears a target and completes after its notifications close.</summary>
        Task ClearAsync(NotificationTarget target, CancellationToken cancellationToken = default);
    }
}
