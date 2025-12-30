using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace LiquidGlassShell.Services
{
    public class NotificationService
    {
        private ObservableCollection<Notification> _notifications = new();
        private const int MaxNotifications = 50;

        public ObservableCollection<Notification> Notifications => _notifications;

        public event EventHandler<Notification>? NotificationAdded;
        public event EventHandler<Notification>? NotificationRemoved;

        public void ShowNotification(string title, string message, NotificationPriority priority = NotificationPriority.Normal)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                Priority = priority,
                Timestamp = DateTime.Now
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                _notifications.Insert(0, notification);
                
                // Limitar cantidad de notificaciones
                if (_notifications.Count > MaxNotifications)
                {
                    _notifications.RemoveAt(_notifications.Count - 1);
                }

                NotificationAdded?.Invoke(this, notification);

                // Auto-remover después de 5 segundos (para normal)
                if (priority == NotificationPriority.Normal)
                {
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(5)
                    };
                    timer.Tick += (s, e) =>
                    {
                        RemoveNotification(notification.Id);
                        timer.Stop();
                    };
                    timer.Start();
                }
            });
        }

        public void RemoveNotification(Guid id)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var notification = _notifications.FirstOrDefault(n => n.Id == id);
                if (notification != null)
                {
                    _notifications.Remove(notification);
                    NotificationRemoved?.Invoke(this, notification);
                }
            });
        }

        public void ClearAll()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _notifications.Clear();
            });
        }
    }

    public class Notification
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationPriority Priority { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }

    public enum NotificationPriority
    {
        Low,
        Normal,
        High,
        Critical
    }
}

