using System.Runtime.InteropServices;

namespace Notifications.Extensions
{
    internal static class WindowExtensions
    {
        #region Window styles : hide alt+tab toastwindow

        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,

            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = -20,

            // ...
        }

        [DllImport("user32.dll")]
        public static extern nint GetWindowLong(this nint hWnd, int nIndex);

        public static nint SetWindowLong(this nint hWnd, int nIndex, nint dwNewLong)
        {
            int error = 0;
            nint result = nint.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (nint.Size == 4)
            {
                // use SetWindowLong
                int tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new nint(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if (result == nint.Zero && error != 0)
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern nint IntSetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int IntSetWindowLong(nint hWnd, int nIndex, int dwNewLong);

        private static int IntPtrToInt32(nint intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);

        #endregion Window styles : hide alt+tab toastwindow
    }
}