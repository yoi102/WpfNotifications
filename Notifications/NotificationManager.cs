﻿using Notifications.Controls;
using System.Windows;
using System.Windows.Threading;

namespace Notifications
{
    public class NotificationManager : INotificationManager
    {
        private static readonly List<NotificationArea> _areas = [];
        private static NotificationsOverlayWindow? _window;
        private readonly Dispatcher _dispatcher;
        public NotificationManager(Dispatcher? dispatcher = null)
        {
            dispatcher ??= Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

            _dispatcher = dispatcher;
        }

        public void Show(Notification content, string areaIdentifier = "", TimeSpan? expirationTime = null, Action? onClick = null,
            Action? onClose = null)
        {
            if (!_dispatcher.CheckAccess())
            {
                _dispatcher.BeginInvoke(
                    new Action(() => Show(content, areaIdentifier, expirationTime, onClick, onClose)));
                return;
            }
            if (content == null)
            {
                throw new Exception();
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
                _window.Closed += (_, _) =>
                {
                    _window = null;
                };
            }

            if (_areas != null && _window is { IsVisible: false })
                _window.Show();

            if (_areas == null) return;

            foreach (var area in _areas.Where(a => a.Identifier == areaIdentifier))
            {
                area.Show(content, (TimeSpan)expirationTime, onClick, onClose);
            }
        }

        internal static void AddArea(NotificationArea area)
        {
            _areas.Add(area);
        }
    }
}