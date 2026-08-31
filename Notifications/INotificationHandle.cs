using Notifications.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Notifications
{
    /// <summary>Controls one displayed notification and observes its lifetime.</summary>
    public interface INotificationHandle
    {
        /// <summary>Gets the unique identifier of this notification instance.</summary>
        Guid Id { get; }

        /// <summary>Gets a task that completes with the dismissal reason.</summary>
        Task<NotificationCloseReason> Completion { get; }

        /// <summary>Closes the notification.</summary>
        Task CloseAsync(CancellationToken cancellationToken = default);

        /// <summary>Replaces the displayed content of an active notification.</summary>
        Task UpdateAsync(object content, CancellationToken cancellationToken = default);
    }
}
