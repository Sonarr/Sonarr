using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers
{
    public class NotificationProvider
    {
        private readonly Dictionary<Guid, BasicNotification> _basicNotifications =
            new Dictionary<Guid, BasicNotification>();

        private readonly Object _lock = new object();

        private readonly Dictionary<Guid, ProgressNotification> _progressNotification =
            new Dictionary<Guid, ProgressNotification>();

        public virtual List<BasicNotification> BasicNotifications
        {
            get { return new List<BasicNotification>(_basicNotifications.Values); }
        }

        public virtual List<ProgressNotification> GetProgressNotifications
        {
            get
            {
                return
                    new List<ProgressNotification>(
                        _progressNotification.Values.Where(p => p.Status == ProgressNotificationStatus.InProgress));
            }
        }

        public virtual void Register(ProgressNotification notification)
        {
            _progressNotification.Add(notification.Id, notification);
        }

        public virtual void Register(BasicNotification notification)
        {
            _basicNotifications.Add(notification.Id, notification);
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