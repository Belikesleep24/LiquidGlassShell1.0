using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using LiquidGlassShell.Services;

namespace LiquidGlassShell.Services
{
    public class AppSwitcherService
    {
        private readonly WindowManagerService _windowManager;
        private readonly TaskManagerService _taskManager;

        public AppSwitcherService(WindowManagerService windowManager, TaskManagerService taskManager)
        {
            _windowManager = windowManager;
            _taskManager = taskManager;
        }

        public void SwitchToNext()
        {
            var apps = _taskManager.GetRunningApps();
            if (apps.Count == 0) return;

            var currentHandle = _windowManager.GetForegroundWindowHandle();
            
            var currentIndex = apps.FindIndex(a => a.Handle == currentHandle);
            var nextIndex = (currentIndex + 1) % apps.Count;

            var nextApp = apps[nextIndex];
            _windowManager.FocusWindow(nextApp.Handle);
        }

        public void SwitchToPrevious()
        {
            var apps = _taskManager.GetRunningApps();
            if (apps.Count == 0) return;

            var currentHandle = _windowManager.GetForegroundWindowHandle();
            
            var currentIndex = apps.FindIndex(a => a.Handle == currentHandle);
            var previousIndex = currentIndex <= 0 ? apps.Count - 1 : currentIndex - 1;

            var previousApp = apps[previousIndex];
            _windowManager.FocusWindow(previousApp.Handle);
        }

        public void SwitchToApp(IntPtr hWnd)
        {
            _windowManager.FocusWindow(hWnd);
        }
    }
}

