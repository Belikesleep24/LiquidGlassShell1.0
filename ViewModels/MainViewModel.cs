using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LiquidGlassShell.Models;
using LiquidGlassShell.Services;

namespace LiquidGlassShell.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
    : this(null)
{
}

        private readonly DockService _dockService;
        private readonly ApplicationLauncherService _launcherService;
        private readonly TaskManagerService _taskManager;
        private readonly TaskbarService _taskbarService;
        private readonly WindowManagerService? _windowManager;
        private ObservableCollection<DockItem> _dockItems = new();
        private ObservableCollection<MenuItem> _menuItems = new();
        private ObservableCollection<TaskbarItem> _taskbarItems = new();

        public MainViewModel(WindowManagerService? windowManager = null)
        {
            _dockService = new DockService();
            _launcherService = new ApplicationLauncherService();
            _taskManager = new TaskManagerService();
            _taskbarService = new TaskbarService(_taskManager, _dockService);
            _windowManager = windowManager;
            LoadDockItems();
            LoadMenuItems();
            LoadTaskbarItems();
            LaunchCommand = new RelayCommand<DockItem>(OnLaunchItem);
            TaskbarItemCommand = new RelayCommand<TaskbarItem>(OnTaskbarItemClick);
            
            // Suscribirse a cambios en TaskbarItems
            _taskbarService.TaskbarItems.CollectionChanged += (s, e) => 
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TaskbarItems = new ObservableCollection<TaskbarItem>(_taskbarService.TaskbarItems);
                });
            };
        }

        public ObservableCollection<DockItem> DockItems
        {
            get => _dockItems;
            set
            {
                _dockItems = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MenuItem> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                OnPropertyChanged();
            }
        }

        public ICommand LaunchCommand { get; }
        public ICommand TaskbarItemCommand { get; }

        public ObservableCollection<TaskbarItem> TaskbarItems
        {
            get => _taskbarItems;
            set
            {
                _taskbarItems = value;
                OnPropertyChanged();
            }
        }

        private void LoadDockItems()
        {
            var items = _dockService.LoadDockItems();
            DockItems = new ObservableCollection<DockItem>(items);
        }

        private void LoadTaskbarItems()
        {
            TaskbarItems = new ObservableCollection<TaskbarItem>(_taskbarService.TaskbarItems);
        }

        private void LoadMenuItems()
        {
            var menus = new ObservableCollection<MenuItem>
            {
                new MenuItem { Title = "LiquidGlassShell", SubItems = new List<MenuItem>
                {
                    new MenuItem { Title = "About LiquidGlassShell", Command = new RelayCommand<object>(_ => ShowAbout()) },
                    new MenuItem { IsSeparator = true },
                    new MenuItem { Title = "Settings...", Command = new RelayCommand<object>(_ => ShowSettings()) },
                    new MenuItem { IsSeparator = true },
                    new MenuItem { Title = "Quit LiquidGlassShell", Command = new RelayCommand<object>(_ => Application.Current.Shutdown()) }
                }},
                new MenuItem { Title = "File", SubItems = new List<MenuItem>
                {
                    new MenuItem { Title = "New Window", Command = new RelayCommand<object>(_ => {}) },
                    new MenuItem { Title = "Open...", Command = new RelayCommand<object>(_ => {}) },
                    new MenuItem { IsSeparator = true },
                    new MenuItem { Title = "Close", Command = new RelayCommand<object>(_ => Application.Current.Shutdown()) }
                }},
                new MenuItem { Title = "Edit", SubItems = new List<MenuItem>
                {
                    new MenuItem { Title = "Undo", Command = new RelayCommand<object>(_ => {}) },
                    new MenuItem { Title = "Redo", Command = new RelayCommand<object>(_ => {}) },
                    new MenuItem { IsSeparator = true },
                    new MenuItem { Title = "Cut", Command = new RelayCommand<object>(_ => {}) },
                    new MenuItem { Title = "Copy", Command = new RelayCommand<object>(_ => {}) },
                    new MenuItem { Title = "Paste", Command = new RelayCommand<object>(_ => {}) }
                }},
                new MenuItem { Title = "View", SubItems = new List<MenuItem>
                {
                    new MenuItem { Title = "Show Dock", IsEnabled = true },
                    new MenuItem { Title = "Show Menu Bar", IsEnabled = true }
                }},
                new MenuItem { Title = "Window", SubItems = new List<MenuItem>
                {
                    new MenuItem { Title = "Minimize", Command = new RelayCommand<object>(_ => {}) },
                    new MenuItem { Title = "Zoom", Command = new RelayCommand<object>(_ => {}) },
                    new MenuItem { IsSeparator = true },
                    new MenuItem { Title = "Bring All to Front", Command = new RelayCommand<object>(_ => {}) }
                }},
                new MenuItem { Title = "Help", SubItems = new List<MenuItem>
                {
                    new MenuItem { Title = "LiquidGlassShell Help", Command = new RelayCommand<object>(_ => {}) }
                }}
            };
            MenuItems = menus;
        }

        private void OnLaunchItem(DockItem? item)
        {
            if (item != null)
            {
                _launcherService.LaunchApplication(item);
            }
        }

        private void ShowAbout()
        {
            MessageBox.Show("LiquidGlassShell\n\nUn shell de escritorio moderno con estilo Liquid Glass.", 
                "About LiquidGlassShell", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowSettings()
        {
            MessageBox.Show("Configuración - Próximamente", 
                "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnTaskbarItemClick(TaskbarItem? item)
        {
            if (item != null)
            {
                if (item.IsRunning && item.WindowHandle != IntPtr.Zero && _windowManager != null)
                {
                    // Enfocar la ventana si está ejecutándose
                    _windowManager.FocusWindow(item.WindowHandle);
                }
                else if (!string.IsNullOrEmpty(item.IconPath))
                {
                    // Si no está ejecutándose, intentar lanzarla desde el dock
                    var dockItem = _dockService.LoadDockItems()
                        .FirstOrDefault(d => d.Name == item.Name || d.IconPath == item.IconPath);
                    if (dockItem != null)
                    {
                        _launcherService.LaunchApplication(dockItem);
                    }
                }
            }
        }
    }
}

