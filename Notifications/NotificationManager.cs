using Notifications.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Notifications
{
    public class NotificationManager : INotificationManager
    {
        private static readonly List<NotificationArea> _areas = new List<NotificationArea>();
#if NETFRAMEWORK
        private static NotificationsOverlayWindow _window;

#else
        private static NotificationsOverlayWindow? _window;
#endif
        private readonly Dispatcher _dispatcher;

#if NETFRAMEWORK
        public NotificationManager(Dispatcher dispatcher = null)

#else
        public NotificationManager(Dispatcher? dispatcher = null)
#endif
        {
            if (dispatcher is null)
            {
                dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            }
            _dispatcher = dispatcher;
        }

        public void Clear(string areaIdentifier = "")
        {
            if (areaIdentifier == string.Empty && _window != null)
            {
                _window.Clear();
            }
            else
            {
                foreach (var area in _areas.Where(a => a.Identifier == areaIdentifier))
                {
                    area.Clear();
                }
            }
        }

#if NETFRAMEWORK

        public void Show(object content,
                               string areaIdentifier = "",
                               bool closeOnClick = true,
                               TimeSpan? expirationTime = null,
                               Action onClick = null,
                               Action onClose = null)

#else
        public void Show(object content,
                               string areaIdentifier = "",
                               bool closeOnClick = true,
                               TimeSpan? expirationTime = null,
                               Action? onClick = null,
                               Action? onClose = null)

#endif
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (!_dispatcher.CheckAccess())
            {
                _dispatcher.BeginInvoke(new Action(
                    () => Show(content, areaIdentifier, closeOnClick, expirationTime, onClick, onClose)));
                return;
            }

            if (expirationTime == null) expirationTime = TimeSpan.FromSeconds(5);

            if (areaIdentifier == string.Empty && _window == null)
            {
                var workArea = SystemParameters.WorkArea;

                _window = new NotificationsOverlayWindow
                {
                    Left = workArea.Left,
                    Top = workArea.Top,
                    Width = workArea.Width,
                    Height = workArea.Height,
                };
                _window.Closed += (sender, args) =>
                {
                    _window = null;
                };
            }

            if (_areas != null && _window != null)
                _window.Show();

            if (_areas == null) return;

            foreach (var area in _areas.Where(a => a.Identifier == areaIdentifier).ToArray())
            {
                area.Show(content, closeOnClick, (TimeSpan)expirationTime, onClick, onClose);
            }
        }

        internal static void AddArea(NotificationArea area)
        {
            _areas.Add(area);
        }
    }
}