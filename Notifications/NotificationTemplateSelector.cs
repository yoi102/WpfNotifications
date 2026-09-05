using System;
using System.Windows;
using System.Windows.Controls;

namespace Notifications
{
    /// <summary>Selects built-in templates for strings and <see cref="NotificationContent"/> values.</summary>
    public class NotificationTemplateSelector : DataTemplateSelector
    {
        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var key = item is string ? "DefaultStringTemplate"
                : item is NotificationContent ? "DefaultNotificationTemplate" : null;
            if (key is null)
            {
                return base.SelectTemplate(item, container)!;
            }

            // A shared selector must resolve resources in the current host's scope.
            if (container is FrameworkElement element && element.TryFindResource(key) is DataTemplate template)
            {
                return template;
            }

            throw new InvalidOperationException($"A DataTemplate resource named '{key}' is required in the notification's resource scope.");
        }
    }
}
