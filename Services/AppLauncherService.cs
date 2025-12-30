using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LiquidGlassShell.Services
{
    public class AppLauncherService
    {
        private readonly List<AppInfo> _installedApps = new();
        private bool _isIndexed = false;

        public async Task IndexApplications()
        {
            if (_isIndexed) return;

            await Task.Run(() =>
            {
                var startMenuPaths = new[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
                };

                foreach (var startMenuPath in startMenuPaths)
                {
                    if (Directory.Exists(startMenuPath))
                    {
                        IndexDirectory(startMenuPath);
                    }
                }
            });

            _isIndexed = true;
        }

        private void IndexDirectory(string directory)
        {
            try
            {
                var lnkFiles = Directory.GetFiles(directory, "*.lnk", SearchOption.AllDirectories);
                foreach (var lnkFile in lnkFiles)
                {
                    try
                    {
                        var appInfo = new AppInfo
                        {
                            Name = Path.GetFileNameWithoutExtension(lnkFile),
                            LnkPath = lnkFile
                        };
                        _installedApps.Add(appInfo);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        public List<AppInfo> SearchApps(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return _installedApps.Take(20).ToList();

            var lowerQuery = query.ToLower();
            return _installedApps
                .Where(app => app.Name.ToLower().Contains(lowerQuery))
                .OrderBy(app => app.Name.ToLower().IndexOf(lowerQuery))
                .Take(20)
                .ToList();
        }

        public void LaunchApp(AppInfo app)
        {
            if (!string.IsNullOrEmpty(app.LnkPath) && File.Exists(app.LnkPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = app.LnkPath,
                    UseShellExecute = true
                });
            }
        }
    }

    public class AppInfo
    {
        public string Name { get; set; } = string.Empty;
        public string LnkPath { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
    }
}

