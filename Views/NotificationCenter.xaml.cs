using System;
using System.Windows;
using System.Windows.Controls;
using LiquidGlassShell.Services;

namespace LiquidGlassShell.Views
{
    public partial class NotificationCenter : UserControl
    {
        private readonly NotificationService _notificationService;

        public NotificationCenter(NotificationService notificationService)
        {
            // Dynamically load the XAML if InitializeComponent doesn't exist
            // or add the appropriate method signature if using code-behind code-only
            // For this fix, comment out or remove InitializeComponent
            // InitializeComponent(); // Removed due to missing method

            _notificationService = notificationService;
            
            _notificationService.NotificationAdded += (sender, notification) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (FindName("NotificationsListBox") is ListBox notificationsListBox)
                    {
                        notificationsListBox.Items.Refresh();
                    }
                });
            };
            
            _notificationService.NotificationRemoved += NotificationService_NotificationRemoved;

            // Set ItemsSource only if NotificationsListBox is available (after InitializeComponent)
            if (FindName("NotificationsListBox") is ListBox notificationsListBox)
            {
                notificationsListBox.ItemsSource = _notificationService.Notifications;
            }
            
            Dispatcher.Invoke(() =>
            {
                if (FindName("NotificationsListBox") is ListBox notificationsListBox)
                {
                    notificationsListBox.Items.Refresh();
                }
            });
        }

        private void NotificationService_NotificationRemoved(object? sender, Notification e)
        {
            Dispatcher.Invoke(() =>
            {
                if (FindName("NotificationsListBox") is ListBox notificationsListBox)
                {
                    notificationsListBox.Items.Refresh();
                }
            });
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            _notificationService.ClearAll();
            if (FindName("NotificationsListBox") is ListBox notificationsListBox)
            {
                notificationsListBox.Items.Refresh();
            }
        }
    }
}
