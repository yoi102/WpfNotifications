using System;

namespace Notifications
{
    /// <summary>Identifies a legacy fire-and-forget manager operation.</summary>
    public enum NotificationManagerOperation
    {
        /// <summary>A legacy show operation.</summary>
        Show,
        /// <summary>A legacy clear operation.</summary>
        Clear,
        /// <summary>Asynchronous cleanup initiated by disposal.</summary>
        Dispose,
    }

    /// <summary>Contains an error raised by a fire-and-forget manager operation.</summary>
    public sealed class NotificationManagerErrorEventArgs : EventArgs
    {
        internal NotificationManagerErrorEventArgs(NotificationManagerOperation operation, Exception exception)
        {
            Operation = operation;
            Exception = exception;
        }

        /// <summary>Gets the operation that failed.</summary>
        public NotificationManagerOperation Operation { get; }

        /// <summary>Gets the reported exception.</summary>
        public Exception Exception { get; }
    }
}
