using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Notifications.Internal
{
    internal readonly struct MonitorDetails
    {
        public MonitorDetails(IntPtr handle, Rect workArea)
        {
            Handle = handle;
            WorkArea = workArea;
        }

        public IntPtr Handle { get; }
        public Rect WorkArea { get; }
    }

    internal static class MonitorHelper
    {
        private const uint MonitorDefaultToPrimary = 1;
        private const uint MonitorDefaultToNearest = 2;

        public static MonitorDetails Resolve(NotificationTarget target)
        {
            IntPtr monitor;
            switch (target.Monitor)
            {
                case NotificationMonitor.MousePointer:
                    GetCursorPos(out var cursor);
                    monitor = MonitorFromPoint(cursor, MonitorDefaultToNearest);
                    break;
                case NotificationMonitor.Owner:
                    var ownerHandle = new WindowInteropHelper(target.Owner!).Handle;
                    monitor = ownerHandle == IntPtr.Zero
                        ? MonitorFromPoint(new NativePoint(), MonitorDefaultToPrimary)
                        : MonitorFromWindow(ownerHandle, MonitorDefaultToNearest);
                    break;
                default:
                    monitor = MonitorFromPoint(new NativePoint(), MonitorDefaultToPrimary);
                    break;
            }

            var info = new MonitorInfo { Size = Marshal.SizeOf(typeof(MonitorInfo)) };
            if (!GetMonitorInfo(monitor, ref info))
            {
                return new MonitorDetails(monitor, SystemParameters.WorkArea);
            }

            var scale = GetMonitorDpi(monitor) / 96d;
            return new MonitorDetails(monitor, new Rect(
                info.WorkArea.Left / scale,
                info.WorkArea.Top / scale,
                (info.WorkArea.Right - info.WorkArea.Left) / scale,
                (info.WorkArea.Bottom - info.WorkArea.Top) / scale));
        }

        private static uint GetMonitorDpi(IntPtr monitor)
        {
            try
            {
                return GetDpiForMonitor(monitor, 0, out var dpiX, out _) == 0 ? dpiX : 96;
            }
            catch (DllNotFoundException)
            {
                return 96;
            }
            catch (EntryPointNotFoundException)
            {
                return 96;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(NativePoint point, uint flags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr window, uint flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out NativePoint point);

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr monitor, int dpiType, out uint dpiX, out uint dpiY);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MonitorInfo
        {
            public int Size;
            public NativeRect Monitor;
            public NativeRect WorkArea;
            public uint Flags;
        }
    }
}
