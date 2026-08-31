namespace Notifications.Enums
{
    /// <summary>Specifies where notifications are anchored inside an area or monitor work area.</summary>
    public enum NotificationPosition
    {
        /// <summary>Top-left corner.</summary>
        TopLeft,
        /// <summary>Top-right corner.</summary>
        TopRight,
        /// <summary>Top edge, horizontally centered.</summary>
        TopCenter,
        /// <summary>Bottom-left corner.</summary>
        BottomLeft,
        /// <summary>Bottom-right corner.</summary>
        BottomRight,
        /// <summary>Bottom edge, horizontally centered.</summary>
        BottomCenter,
        /// <summary>Left edge, vertically centered.</summary>
        CenterLeft,
        /// <summary>Right edge, vertically centered.</summary>
        CenterRight,
        /// <summary>Horizontal and vertical center.</summary>
        Center
    }
}
