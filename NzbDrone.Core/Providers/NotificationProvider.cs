using System;
using System.Collections.Generic;
using NzbDrone.Core.Entities.Notification;

namespace NzbDrone.Core.Providers
{
    class NotificationProvider : INotificationProvider
    {
        private readonly Dictionary<Guid, BasicNotification> _basicNotifications = new Dictionary<Guid, BasicNotification>();
        private readonly Dictionary<Guid, ProgressNotification> _progressNotification = new Dictionary<Guid, ProgressNotification>();
        private readonly Object _lock = new object();

        public void Register(ProgressNotification notification)
        {
            _progressNotification.Add(notification.Id, notification);
        }

        public void Register(BasicNotification notification)
        {
            _basicNotifications.Add(notification.Id, notification);
        }

        public List<BasicNotification> BasicNotifications
        {
            get { return new List<BasicNotification>(_basicNotifications.Values); }
        }

        public List<ProgressNotification> ProgressNotifications
        {
            get { return new List<ProgressNotification>(_progressNotification.Values); }
        }

        public void Dismiss(Guid notificationId)
        {
            lock (_lock)
            {
                if (_basicNotifications.ContainsKey(notificationId))
                {
                    _basicNotifications.Remove(notificationId);
                }
                else if (_progressNotification.ContainsKey(notificationId))
                {
                    _progressNotification.Remove(notificationId);
                }
            }
        }
    }
}