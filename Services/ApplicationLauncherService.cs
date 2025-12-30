using System;
using System.Diagnostics;
using System.IO;
using LiquidGlassShell.Models;

namespace LiquidGlassShell.Services
{
    public class ApplicationLauncherService
    {
        public bool LaunchApplication(DockItem item)
        {
            if (item == null)
            {
                return false;
            }

            try
            {
                string fileName;
                
                if (!string.IsNullOrEmpty(item.ApplicationName))
                {
                    fileName = item.ApplicationName;
                }
                else if (!string.IsNullOrEmpty(item.ExecutablePath) && File.Exists(item.ExecutablePath))
                {
                    fileName = item.ExecutablePath;
                }
                else
                {
                    return false;
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                if (!string.IsNullOrEmpty(item.Arguments))
                {
                    processStartInfo.Arguments = item.Arguments;
                }

                if (!string.IsNullOrEmpty(item.WorkingDirectory) && Directory.Exists(item.WorkingDirectory))
                {
                    processStartInfo.WorkingDirectory = item.WorkingDirectory;
                }

                Process.Start(processStartInfo);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error launching application: {ex.Message}");
                return false;
            }
        }

    }
}

