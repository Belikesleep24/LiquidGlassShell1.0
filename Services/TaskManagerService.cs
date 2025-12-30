using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LiquidGlassShell.Models;

namespace LiquidGlassShell.Services
{
    public class TaskManagerService
    {
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private List<RunningApp> _runningApps = new();
        private Timer? _refreshTimer;

        public event EventHandler? RunningAppsChanged;

        public void StartMonitoring()
        {
            RefreshRunningApps();
            _refreshTimer = new Timer(_ => RefreshRunningApps(), null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        }

        public void StopMonitoring()
        {
            _refreshTimer?.Dispose();
        }

        public List<RunningApp> GetRunningApps()
        {
            return _runningApps.ToList();
        }

        private void RefreshRunningApps()
        {
            var apps = new List<RunningApp>();
            var handles = new HashSet<IntPtr>();

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd) && !handles.Contains(hWnd))
                {
                    var title = GetWindowTitle(hWnd);
                    if (!string.IsNullOrEmpty(title) && title != "Program Manager")
                    {
                        GetWindowThreadProcessId(hWnd, out uint processId);
                        try
                        {
                            var process = Process.GetProcessById((int)processId);
                            apps.Add(new RunningApp
                            {
                                Handle = hWnd,
                                Title = title,
                                ProcessName = process.ProcessName,
                                ProcessId = processId
                            });
                            handles.Add(hWnd);
                        }
                        catch
                        {
                        }
                    }
                }
                return true;
            }, IntPtr.Zero);

            _runningApps = apps;
            RunningAppsChanged?.Invoke(this, EventArgs.Empty);
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
    }

    public class RunningApp
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public uint ProcessId { get; set; }
        public bool IsMinimized { get; set; }
        public bool HasAttention { get; set; }
    }
}

