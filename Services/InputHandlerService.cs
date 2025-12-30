using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace LiquidGlassShell.Services
{
    public class InputHandlerService
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CTRL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const int WM_HOTKEY = 0x0312;

        private IntPtr _windowHandle;
        private HwndSource? _source;

        public event EventHandler<KeyEventArgs>? GlobalKeyPressed;
        public event EventHandler? EscapePressed;

        public void Initialize(Window window)
        {
            var helper = new WindowInteropHelper(window);
            _windowHandle = helper.EnsureHandle();
            _source = HwndSource.FromHwnd(_windowHandle);
            _source?.AddHook(WndProc);

            window.KeyDown += Window_KeyDown;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                EscapePressed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                GlobalKeyPressed?.Invoke(this, e);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                handled = true;
            }
            return IntPtr.Zero;
        }

        public bool RegisterGlobalHotkey(int id, ModifierKeys modifiers, Key key)
        {
            uint modFlags = 0;
            if (modifiers.HasFlag(ModifierKeys.Alt)) modFlags |= MOD_ALT;
            if (modifiers.HasFlag(ModifierKeys.Control)) modFlags |= MOD_CTRL;
            if (modifiers.HasFlag(ModifierKeys.Shift)) modFlags |= MOD_SHIFT;
            if (modifiers.HasFlag(ModifierKeys.Windows)) modFlags |= MOD_WIN;

            return RegisterHotKey(_windowHandle, id, modFlags, (uint)KeyInterop.VirtualKeyFromKey(key));
        }

        public void UnregisterGlobalHotkey(int id)
        {
            UnregisterHotKey(_windowHandle, id);
        }

        public void Cleanup()
        {
            _source?.RemoveHook(WndProc);
            _source?.Dispose();
        }
    }
}

