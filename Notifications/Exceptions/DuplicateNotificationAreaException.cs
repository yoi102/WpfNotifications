using System;

namespace Notifications.Exceptions
{
    /// <summary>Thrown when multiple loaded notification areas use the same identifier.</summary>
    public sealed class DuplicateNotificationAreaException : InvalidOperationException
    {
        /// <summary>Initializes the exception for a duplicated area identifier.</summary>
        public DuplicateNotificationAreaException(string areaIdentifier)
            : base($"More than one loaded NotificationArea has the identifier '{areaIdentifier}'. Identifiers must be unique per Dispatcher.")
        {
            AreaIdentifier = areaIdentifier;
        }

        /// <summary>Gets the duplicated area identifier.</summary>
        public string AreaIdentifier { get; }
    }
}
