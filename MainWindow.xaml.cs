using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using LiquidGlassShell.Services;

namespace LiquidGlassShell
{
    public partial class MainWindow : Window
    {
        private readonly WindowManagerService _windowManager;
        private readonly DesktopManagerService _desktopManager;
        private readonly InputHandlerService _inputHandler;
        private readonly SystemIntegrationService _systemIntegration;
        private readonly TaskManagerService _taskManager;
        private readonly AppSwitcherService _appSwitcher;
        private readonly WindowSnappingService _windowSnapping;
        private readonly AppLauncherService _appLauncher;
        private readonly DockService _dockService;
        private readonly TaskbarService _taskbarService;
        private readonly SearchService _searchService;
        private readonly NotificationService _notificationService;
        private readonly FileExplorerService _fileExplorerService;
        private DispatcherTimer? _clockTimer;
        private Views.SearchWindow? _searchWindow;

        public MainWindow()
        {
#if !DEBUG
            InitializeComponent();
#endif

            _windowManager = new WindowManagerService();
            _desktopManager = new DesktopManagerService();
            _inputHandler = new InputHandlerService();
            _systemIntegration = new SystemIntegrationService();
            _taskManager = new TaskManagerService();
            _appSwitcher = new AppSwitcherService(_windowManager, _taskManager);
            _windowSnapping = new WindowSnappingService();
            _appLauncher = new AppLauncherService();
            _dockService = new DockService();
            _taskbarService = new TaskbarService(_taskManager, _dockService);
            _searchService = new SearchService(_appLauncher);
            _notificationService = new NotificationService();
            _fileExplorerService = new FileExplorerService();

            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _desktopManager.ConfigureShellWindow(this);
            _windowManager.Initialize(this);
            _inputHandler.Initialize(this);
            
            _inputHandler.EscapePressed += InputHandler_EscapePressed;
            _inputHandler.GlobalKeyPressed += InputHandler_GlobalKeyPressed;
            
            _taskManager.StartMonitoring();
            _taskbarService.TaskbarItems.CollectionChanged += TaskbarItems_CollectionChanged;
            
            // Actualizar ViewModel con WindowManagerService si es necesario
            if (this.DataContext is ViewModels.MainViewModel viewModel)
            {
                try
                {
                    // Inyectar WindowManagerService usando reflexión
                    var windowManagerField = typeof(ViewModels.MainViewModel).GetField("_windowManager", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    windowManagerField?.SetValue(viewModel, _windowManager);
                }
                catch
                {
                    // Si falla la inyección, continuar sin ella
                }
            }
            
            _ = _appLauncher.IndexApplications();
            
            // Indexar archivos para búsqueda
            _ = _searchService.IndexFilesAsync(new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            });
            
            // Aplicar efecto Liquid Glass real usando APIs de Windows
            ApplyLiquidGlassEffect();
            
            // Inicializar reloj
            InitializeClock();
            
            // Mostrar notificación de bienvenida
            _notificationService.ShowNotification(
                "LiquidGlassShell",
                "Shell activo. Presiona Win+S para buscar.",
                NotificationPriority.Normal);
            
            this.Focus();
        }

        private void ApplyLiquidGlassEffect()
        {
            try
            {
                // Intentar aplicar Acrylic (Liquid Glass real) - Windows 10/11
                var glassColor = Color.FromArgb(255, 255, 255, 255); // Blanco translúcido
                LiquidGlassHelper.ApplyAcrylicEffect(this, glassColor, 20); // Opacidad muy baja para efecto glass
            }
            catch
            {
                // Si Acrylic no está disponible, usar Blur Behind
                try
                {
                    var glassColor = Color.FromArgb(255, 255, 255, 255);
                    LiquidGlassHelper.ApplyBlurBehindEffect(this, glassColor, 30);
                }
                catch
                {
                    // Si ambos fallan, usar el efecto visual de WPF
                }
            }
        }
        
        private void InitializeClock()
        {
            UpdateClock();
            
            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += (s, e) => UpdateClock();
            _clockTimer.Start();
        }
        
        private void UpdateClock()
        {
            var now = DateTime.Now;

            // Actualizar hora
            if (this.FindName("TimeTextBlock") is System.Windows.Controls.TextBlock timeTextBlock)
            {
                timeTextBlock.Text = now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            }


            // Actualizar fecha
            if (this.FindName("DateTextBlock") is System.Windows.Controls.TextBlock dateTextBlock)
            {
                var culture = new CultureInfo("es-ES");
                dateTextBlock.Text = now.ToString("dddd, d 'de' MMMM", culture);
            }

        }

        private void TaskbarItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Actualizar el ViewModel cuando cambien los TaskbarItems
            if (this.DataContext is ViewModels.MainViewModel viewModel)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    viewModel.TaskbarItems = new System.Collections.ObjectModel.ObservableCollection<Services.TaskbarItem>(_taskbarService.TaskbarItems);
                });
            }
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            _clockTimer?.Stop();
            _taskManager.StopMonitoring();
            _inputHandler.Cleanup();
        }

        private void InputHandler_EscapePressed(object? sender, System.EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void InputHandler_GlobalKeyPressed(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                e.Handled = true;
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    _appSwitcher.SwitchToPrevious();
                }
                else
                {
                    _appSwitcher.SwitchToNext();
                }
            }
            else if (e.Key == Key.Left && Keyboard.Modifiers == ModifierKeys.Windows)
            {
                var hWnd = _windowManager.GetForegroundWindowHandle();
                if (hWnd != IntPtr.Zero)
                {
                    _windowSnapping.SnapLeft(hWnd);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Right && Keyboard.Modifiers == ModifierKeys.Windows)
            {
                var hWnd = _windowManager.GetForegroundWindowHandle();
                if (hWnd != IntPtr.Zero)
                {
                    _windowSnapping.SnapRight(hWnd);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                // Ctrl+Shift+D para probar DesktopManagerService
                TestDesktopManagerService();
                e.Handled = true;
            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Windows)
            {
                // Win+S para búsqueda global
                ShowSearchWindow();
                e.Handled = true;
            }
            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Windows)
            {
                // Win+D para mostrar escritorio (minimizar todas las ventanas)
                ShowDesktop();
                e.Handled = true;
            }
            else if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Windows)
            {
                // Win+L para bloquear
                LockWorkstation();
                e.Handled = true;
            }
        }

        private void TestDesktopManagerService()
        {
            try
            {
                // Probar GetMonitorCount
                int monitorCount = _desktopManager.GetMonitorCount();
                
                // Probar GetDpiScale
                double dpiScale = _desktopManager.GetDpiScale();
                
                // Probar GetPrimaryScreenBounds
                Rect screenBounds = _desktopManager.GetPrimaryScreenBounds();
                
                // Mostrar resultados
                string message = $"Resultados de DesktopManagerService:\n\n" +
                               $"• Número de monitores: {monitorCount}\n" +
                               $"• Escala DPI: {dpiScale:F2}\n" +
                               $"• Límites de pantalla principal:\n" +
                               $"  - X: {screenBounds.X:F0}\n" +
                               $"  - Y: {screenBounds.Y:F0}\n" +
                               $"  - Ancho: {screenBounds.Width:F0}\n" +
                               $"  - Alto: {screenBounds.Height:F0}\n\n" +
                               $"• ConfigureShellWindow: Ya aplicado en la ventana principal";
                
                System.Windows.MessageBox.Show(message, "Prueba de DesktopManagerService", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al probar DesktopManagerService:\n{ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("LiquidGlassShell\n\nUn shell de escritorio moderno con estilo Liquid Glass.\n\nVersión 1.0", 
                "About LiquidGlassShell", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Configuración - Próximamente", 
                "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void QuitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowShortcutsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Atajos de teclado:\n\n" +
                "• Alt + Tab - Cambiar entre aplicaciones\n" +
                "• Win + ← / → - Ajustar ventanas\n" +
                "• Ctrl + Shift + D - Probar DesktopManagerService\n" +
                "• ESC - Salir\n" +
                "• F11 - Pantalla completa",
                "Keyboard Shortcuts", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }

        private void ChangeWallpaperMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar Fondo de Pantalla",
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Todos los archivos|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string imagePath = openFileDialog.FileName;
                    bool success = _desktopManager.SetWallpaper(imagePath);
                    
                    if (success)
                    {
                        MessageBox.Show(
                            $"Fondo de pantalla cambiado exitosamente.\n\n{Path.GetFileName(imagePath)}",
                            "Fondo de Pantalla",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "No se pudo cambiar el fondo de pantalla. Por favor, verifica que el archivo sea válido.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al cambiar el fondo de pantalla:\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void WallpaperGalleryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Crear carpeta de wallpapers si no existe
            string wallpapersFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "LiquidGlassShell",
                "Wallpapers");

            if (!Directory.Exists(wallpapersFolder))
            {
                try
                {
                    Directory.CreateDirectory(wallpapersFolder);
                    MessageBox.Show(
                        $"Carpeta de fondos de pantalla creada:\n{wallpapersFolder}\n\nColoca tus imágenes allí y luego selecciona 'Change Wallpaper…' para usarlas.",
                        "Galería de Fondos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al crear la carpeta:\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(
                    $"Carpeta de fondos de pantalla:\n{wallpapersFolder}\n\nColoca tus imágenes allí y luego selecciona 'Change Wallpaper…' para usarlas.",
                    "Galería de Fondos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ShowSearchWindow()
        {
            if (_searchWindow == null)
            {
                _searchWindow = new Views.SearchWindow(_searchService);
                _searchWindow.Owner = this;
            }
            
            _searchWindow.Left = (SystemParameters.PrimaryScreenWidth - _searchWindow.Width) / 2;
            _searchWindow.Top = 200;
            _searchWindow.Show();
            _searchWindow.Activate();
        }

        private void ShowDesktop()
        {
            // Minimizar todas las ventanas excepto el shell
            var windows = _windowManager.GetOpenWindows();
            foreach (var window in windows)
            {
                if (window.Handle != new System.Windows.Interop.WindowInteropHelper(this).Handle)
                {
                    _windowManager.MinimizeWindow(window.Handle);
                }
            }
        }

        private void LockWorkstation()
        {
            _systemIntegration.LockSession();
        }
    }
}

