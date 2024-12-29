using System.Windows;
using System.Windows.Media;

namespace Notifications.Extensions
{
    internal class VisualTreeHelperExtensions
    {
#if NETFRAMEWORK
        public static T GetParent<T>(DependencyObject child) where T : DependencyObject

#else
        public static T? GetParent<T>(DependencyObject child) where T : DependencyObject
#endif
        {
            var parent = VisualTreeHelper.GetParent(child);

            if (parent == null) return null;

            if (parent is T tParent)
            {
                return tParent;
            }

            return GetParent<T>(parent);
        }
    }
}