using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace LiquidGlassShell.Services
{
    public class ProcessManagerService
    {
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public List<ProcessInfo> GetRunningProcesses()
        {
            var processes = new List<ProcessInfo>();
            
            try
            {
                var allProcesses = Process.GetProcesses();
                foreach (var process in allProcesses)
                {
                    try
                    {
                        processes.Add(new ProcessInfo
                        {
                            ProcessId = process.Id,
                            ProcessName = process.ProcessName,
                            WindowTitle = process.MainWindowTitle,
                            MemoryUsage = process.WorkingSet64,
                            CpuUsage = 0, // Requiere cálculo adicional
                            StartTime = process.StartTime
                        });
                    }
                    catch
                    {
                        // Ignorar procesos sin acceso
                    }
                }
            }
            catch
            {
            }

            return processes.OrderBy(p => p.ProcessName).ToList();
        }

        public bool KillProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool KillProcess(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    process.Kill();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class ProcessInfo
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public long MemoryUsage { get; set; }
        public double CpuUsage { get; set; }
        public DateTime StartTime { get; set; }
    }
}

