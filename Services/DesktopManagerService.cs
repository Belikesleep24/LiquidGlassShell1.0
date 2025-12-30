using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace LiquidGlassShell.Services
{
    public class DesktopManagerService
    {
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SM_CMONITORS = 80;
        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;
        private const int SPI_SETDESKWALLPAPER = 0x0014;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        public int GetMonitorCount()
        {
            return GetSystemMetrics(SM_CMONITORS);
        }

        public double GetDpiScale()
        {
            var hdc = GetDC(IntPtr.Zero);
            try
            {
                var dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
                return dpiX / 96.0;
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        public Rect GetPrimaryScreenBounds()
        {
            return SystemParameters.WorkArea;
        }

        public void ConfigureShellWindow(Window shellWindow)
        {
            shellWindow.WindowState = WindowState.Maximized;
            
            var dpiScale = GetDpiScale();
            if (dpiScale != 1.0)
            {
                shellWindow.LayoutTransform = new ScaleTransform(dpiScale, dpiScale);
            }
        }

        public bool SetWallpaper(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                {
                    return false;
                }

                // Cambiar el fondo de pantalla
                int result = SystemParametersInfo(
                    SPI_SETDESKWALLPAPER,
                    0,
                    imagePath,
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

                return result != 0;
            }
            catch
            {
                return false;
            }
        }

        public string? GetCurrentWallpaper()
        {
            try
            {
                // Obtener la ruta del fondo de pantalla actual desde el registro
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
                {
                    if (key != null)
                    {
                        var wallpaper = key.GetValue("WallPaper") as string;
                        if (!string.IsNullOrEmpty(wallpaper) && File.Exists(wallpaper))
                        {
                            return wallpaper;
                        }
                    }
                }
            }
            catch
            {
            }
            return null;
        }
    }
}

