using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers
{
    public class NotificationProvider
    {
        private readonly Dictionary<Guid, BasicNotification> _basicNotifications = new Dictionary<Guid, BasicNotification>();
        private Dictionary<Guid, ProgressNotification> _progressNotification = new Dictionary<Guid, ProgressNotification>();
        private readonly Object _lock = new object();

        public virtual void Register(ProgressNotification notification)
        {
            _progressNotification.Add(notification.Id, notification);
        }

        public virtual void Register(BasicNotification notification)
        {
            _basicNotifications.Add(notification.Id, notification);
        }

        public virtual List<BasicNotification> BasicNotifications
        {
            get { return new List<BasicNotification>(_basicNotifications.Values); }
        }

        public virtual List<ProgressNotification> GetProgressNotifications
        {
            get
            {
                return new List<ProgressNotification>(_progressNotification.Values.Where(p => p.Status == ProgressNotificationStatus.InProgress));
            }
        }

        public virtual void Dismiss(Guid notificationId)
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