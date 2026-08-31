using System;

namespace Notifications.Exceptions
{
    /// <summary>Thrown when no loaded notification area matches a requested identifier.</summary>
    public sealed class NotificationAreaNotFoundException : InvalidOperationException
    {
        /// <summary>Initializes the exception for an area identifier.</summary>
        public NotificationAreaNotFoundException(string areaIdentifier)
            : base($"No loaded NotificationArea has the identifier '{areaIdentifier}'.")
        {
            AreaIdentifier = areaIdentifier;
        }

        /// <summary>Gets the requested area identifier.</summary>
        public string AreaIdentifier { get; }
    }
}
