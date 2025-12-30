using System;
using System.Runtime.InteropServices;

namespace LiquidGlassShell.Services
{
    public class WindowSnappingService
    {
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public void SnapLeft(IntPtr hWnd)
        {
            var screenWidth = GetSystemMetrics(SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SM_CYSCREEN);
            var halfWidth = screenWidth / 2;

            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, halfWidth, screenHeight, SWP_SHOWWINDOW);
        }

        public void SnapRight(IntPtr hWnd)
        {
            var screenWidth = GetSystemMetrics(SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SM_CYSCREEN);
            var halfWidth = screenWidth / 2;

            SetWindowPos(hWnd, IntPtr.Zero, halfWidth, 0, halfWidth, screenHeight, SWP_SHOWWINDOW);
        }

        public void SnapTopLeft(IntPtr hWnd)
        {
            var screenWidth = GetSystemMetrics(SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SM_CYSCREEN);
            var halfWidth = screenWidth / 2;
            var halfHeight = screenHeight / 2;

            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, halfWidth, halfHeight, SWP_SHOWWINDOW);
        }

        public void SnapTopRight(IntPtr hWnd)
        {
            var screenWidth = GetSystemMetrics(SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SM_CYSCREEN);
            var halfWidth = screenWidth / 2;
            var halfHeight = screenHeight / 2;

            SetWindowPos(hWnd, IntPtr.Zero, halfWidth, 0, halfWidth, halfHeight, SWP_SHOWWINDOW);
        }

        public void SnapBottomLeft(IntPtr hWnd)
        {
            var screenWidth = GetSystemMetrics(SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SM_CYSCREEN);
            var halfWidth = screenWidth / 2;
            var halfHeight = screenHeight / 2;

            SetWindowPos(hWnd, IntPtr.Zero, 0, halfHeight, halfWidth, halfHeight, SWP_SHOWWINDOW);
        }

        public void SnapBottomRight(IntPtr hWnd)
        {
            var screenWidth = GetSystemMetrics(SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SM_CYSCREEN);
            var halfWidth = screenWidth / 2;
            var halfHeight = screenHeight / 2;

            SetWindowPos(hWnd, IntPtr.Zero, halfWidth, halfHeight, halfWidth, halfHeight, SWP_SHOWWINDOW);
        }

        public void Maximize(IntPtr hWnd)
        {
            var screenWidth = GetSystemMetrics(SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SM_CYSCREEN);

            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, screenWidth, screenHeight, SWP_SHOWWINDOW);
        }
    }
}

