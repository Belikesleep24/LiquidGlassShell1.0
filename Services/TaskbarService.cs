using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using LiquidGlassShell.Models;

namespace LiquidGlassShell.Services
{
    public class TaskbarService
    {
        private readonly TaskManagerService _taskManager;
        private readonly DockService _dockService;
        private ObservableCollection<TaskbarItem> _taskbarItems = new();

        public TaskbarService(TaskManagerService taskManager, DockService dockService)
        {
            _taskManager = taskManager;
            _dockService = dockService;
            _taskManager.RunningAppsChanged += TaskManager_RunningAppsChanged;
        }

        public ObservableCollection<TaskbarItem> TaskbarItems => _taskbarItems;

        private void TaskManager_RunningAppsChanged(object? sender, EventArgs e)
        {
            UpdateTaskbarItems();
        }

        private void UpdateTaskbarItems()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var runningApps = _taskManager.GetRunningApps();
                var dockItems = _dockService.LoadDockItems();
                
                var items = new List<TaskbarItem>();

                // Agregar apps pinned del dock
                foreach (var dockItem in dockItems)
                {
                    var runningApp = runningApps.FirstOrDefault(a => 
                        a.ProcessName.Equals(dockItem.ApplicationName, StringComparison.OrdinalIgnoreCase) ||
                        a.Title.Contains(dockItem.Name, StringComparison.OrdinalIgnoreCase));

                    items.Add(new TaskbarItem
                    {
                        Name = dockItem.Name,
                        IconPath = dockItem.IconPath,
                        IsPinned = true,
                        IsRunning = runningApp != null,
                        WindowHandle = runningApp?.Handle ?? IntPtr.Zero,
                        ProcessId = runningApp?.ProcessId ?? 0,
                        WindowCount = runningApps.Count(a => a.ProcessName == dockItem.ApplicationName)
                    });
                }

                // Agregar otras apps running que no están en el dock
                foreach (var app in runningApps)
                {
                    if (!items.Any(i => i.WindowHandle == app.Handle))
                    {
                        items.Add(new TaskbarItem
                        {
                            Name = app.Title,
                            IsPinned = false,
                            IsRunning = true,
                            WindowHandle = app.Handle,
                            ProcessId = app.ProcessId,
                            WindowCount = 1
                        });
                    }
                }

                _taskbarItems = new ObservableCollection<TaskbarItem>(items);
            });
        }

        public void PinApp(string name, string iconPath, string executablePath)
        {
            // Agregar al dock.json
            // Implementar lógica de pin
        }

        public void UnpinApp(string name)
        {
            // Remover del dock.json
        }
    }

    public class TaskbarItem
    {
        public string Name { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public bool IsPinned { get; set; }
        public bool IsRunning { get; set; }
        public IntPtr WindowHandle { get; set; }
        public uint ProcessId { get; set; }
        public int WindowCount { get; set; }
    }
}

