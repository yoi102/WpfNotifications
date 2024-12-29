using System;
using System.Windows;
using System.Windows.Controls;

namespace Notifications
{
    public class NotificationTemplateSelector : DataTemplateSelector
    {
#if NETFRAMEWORK
        private DataTemplate _defaultNotificationTemplate;
        private DataTemplate _defaultStringTemplate;
#else
        private DataTemplate? _defaultNotificationTemplate;
        private DataTemplate? _defaultStringTemplate;
#endif

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (_defaultStringTemplate == null && _defaultNotificationTemplate == null)
            {
                GetTemplatesFromResources((FrameworkElement)container);
            }

            if (item is string)
            {
                if (_defaultStringTemplate == null)
                {
                    throw new ArgumentNullException(nameof(_defaultStringTemplate));
                }

                return _defaultStringTemplate;
            }
            if (item is NotificationContent)
            {
                if (_defaultNotificationTemplate == null)
                {
                    throw new ArgumentNullException(nameof(_defaultNotificationTemplate));
                }
                return _defaultNotificationTemplate;
            }

            return base.SelectTemplate(item, container);
        }

        private void GetTemplatesFromResources(FrameworkElement container)
        {
            _defaultStringTemplate =
                    container?.FindResource("DefaultStringTemplate") as DataTemplate;

            _defaultNotificationTemplate =
                    container?.FindResource("DefaultNotificationTemplate") as DataTemplate;
        }
    }
}