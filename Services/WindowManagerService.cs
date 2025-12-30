using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace LiquidGlassShell.Services
{
    public class WindowManagerService
    {
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsZoomed(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        private const int SW_MAXIMIZE = 3;
        private const int SW_RESTORE = 9;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint GW_HWNDNEXT = 2;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private List<IntPtr> _managedWindows = new();
        private IntPtr _shellWindowHandle;

        public void Initialize(Window shellWindow)
        {
            var helper = new WindowInteropHelper(shellWindow);
            _shellWindowHandle = helper.EnsureHandle();
        }

        public List<WindowInfo> GetOpenWindows()
        {
            var windows = new List<WindowInfo>();
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd) && hWnd != _shellWindowHandle)
                {
                    var title = GetWindowTitle(hWnd);
                    if (!string.IsNullOrEmpty(title))
                    {
                        windows.Add(new WindowInfo
                        {
                            Handle = hWnd,
                            Title = title
                        });
                    }
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        public void FocusWindow(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
                BringWindowToTop(hWnd);
            }
        }

        public void MinimizeWindow(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_MINIMIZE);
            }
        }

        public void MaximizeWindow(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_MAXIMIZE);
            }
        }

        public void RestoreWindow(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_RESTORE);
            }
        }

        public IntPtr GetForegroundWindowHandle()
        {
            return GetForegroundWindow();
        }

        public void SetWindowZOrder(IntPtr hWnd, bool topMost)
        {
            if (hWnd != IntPtr.Zero)
            {
                SetWindowPos(hWnd, topMost ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }
        }

        private string GetWindowTitle(IntPtr hWnd)
        {
            var length = GetWindowTextLength(hWnd);
            if (length == 0) return string.Empty;

            var builder = new System.Text.StringBuilder(length + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        public void RegisterManagedWindow(IntPtr hWnd)
        {
            if (!_managedWindows.Contains(hWnd))
            {
                _managedWindows.Add(hWnd);
            }
        }

        public void MoveWindow(IntPtr hWnd, int x, int y)
        {
            if (hWnd != IntPtr.Zero)
            {
                if (GetWindowRect(hWnd, out RECT rect))
                {
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;
                    MoveWindow(hWnd, x, y, width, height, true);
                }
            }
        }

        public void ResizeWindow(IntPtr hWnd, int width, int height)
        {
            if (hWnd != IntPtr.Zero)
            {
                if (GetWindowRect(hWnd, out RECT rect))
                {
                    MoveWindow(hWnd, rect.Left, rect.Top, width, height, true);
                }
            }
        }

        public bool IsWindowMinimized(IntPtr hWnd)
        {
            return hWnd != IntPtr.Zero && IsIconic(hWnd);
        }

        public bool IsWindowMaximized(IntPtr hWnd)
        {
            return hWnd != IntPtr.Zero && IsZoomed(hWnd);
        }

        public void CloseWindow(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                SendMessage(hWnd, 0x0010, IntPtr.Zero, IntPtr.Zero); // WM_CLOSE
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    }

    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
    }
}

