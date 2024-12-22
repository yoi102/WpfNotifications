using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Notifications
{
    internal class VisualTreeHelperExtensions
    {
        public static T? GetParent<T>(DependencyObject child) where T : DependencyObject
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
