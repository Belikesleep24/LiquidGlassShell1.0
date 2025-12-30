using System;
using System.Runtime.InteropServices;

namespace LiquidGlassShell.Services
{
    public class SystemIntegrationService
    {
        [DllImport("user32.dll")]
        private static extern bool LockWorkStation();

        [DllImport("user32.dll")]
        private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        private const uint EWX_LOGOFF = 0x00000000;
        private const uint EWX_SHUTDOWN = 0x00000001;
        private const uint EWX_REBOOT = 0x00000002;
        private const uint EWX_FORCE = 0x00000004;

        public void LockSession()
        {
            LockWorkStation();
        }

        public SystemInfo GetSystemInfo()
        {
            return new SystemInfo
            {
                NetworkConnected = IsNetworkConnected(),
                VolumeLevel = GetSystemVolume()
            };
        }

        private bool IsNetworkConnected()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }

        private int GetSystemVolume()
        {
            return 50;
        }
    }

    public class SystemInfo
    {
        public bool NetworkConnected { get; set; }
        public int VolumeLevel { get; set; }
    }
}

