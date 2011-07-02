using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers
{
    public class NotificationProvider
    {
        private static readonly Dictionary<Guid, BasicNotification> _basicNotifications =
            new Dictionary<Guid, BasicNotification>();

        private static readonly Object _lock = new object();

        private static readonly Dictionary<Guid, ProgressNotification> _progressNotification =
            new Dictionary<Guid, ProgressNotification>();

        public virtual List<BasicNotification> BasicNotifications
        {
            get { return new List<BasicNotification>(_basicNotifications.Values); }
        }

        public virtual List<ProgressNotification> ProgressNotifications
        {
            get
            {

                var activeNotification = _progressNotification.Values.Where(p => p.Status == ProgressNotificationStatus.InProgress).ToList();

                if (activeNotification.Count == 0)
                {
                    //Get notifications that were recently done
                    activeNotification = _progressNotification.Values.Where(p => p.CompletedTime >= DateTime.Now.AddSeconds(-3)).OrderByDescending(c => c.CompletedTime).ToList();

                }

                return activeNotification.ToList();
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