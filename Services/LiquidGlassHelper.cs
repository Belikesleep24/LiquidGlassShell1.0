using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace LiquidGlassShell
{
    public static class LiquidGlassHelper
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("dwmapi.dll")]
        private static extern void DwmEnableBlurBehindWindow(IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public uint dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        public static void ApplyAcrylicEffect(Window window, Color glassColor, byte opacity)
        {
            try
            {
                var helper = new WindowInteropHelper(window);
                ApplyWindowChrome(helper.Handle);
                EnableBlurBehind(helper.Handle);
            }
            catch { }
        }

        public static void ApplyBlurBehindEffect(Window window, Color glassColor, byte opacity)
        {
            try
            {
                var helper = new WindowInteropHelper(window);
                EnableBlurBehind(helper.Handle);
            }
            catch { }
        }

        private static void ApplyWindowChrome(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
        }

        private static void EnableBlurBehind(IntPtr hWnd)
        {
            var blur = new DWM_BLURBEHIND
            {
                dwFlags = 1,
                fEnable = true,
                hRgnBlur = IntPtr.Zero,
                fTransitionOnMaximized = false
            };

            DwmEnableBlurBehindWindow(hWnd, ref blur);
        }

        public static void SetDarkMode(IntPtr hWnd, bool enable)
        {
            try
            {
                int darkMode = enable ? 1 : 0;
                DwmSetWindowAttribute(hWnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
            }
            catch { }
        }
    }
}