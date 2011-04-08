using System;
using System.Collections.Generic;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers.Fakes
{
    class FakeNotificationProvider
    {
        private readonly Dictionary<Guid, BasicNotification> _basicNotifications = new Dictionary<Guid, BasicNotification>();
        private readonly Dictionary<Guid, ProgressNotification> _progressNotification = new Dictionary<Guid, ProgressNotification>();
        private readonly Object _lock = new object();


        ProgressNotification fakeNotification = new ProgressNotification("Updating Series");
        ProgressNotification fakeNotification2 = new ProgressNotification("Updating Series2");
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

        public List<ProgressNotification> GetProgressNotifications
        {

            get
            {
                fakeNotification.Status = ProgressNotificationStatus.InProgress;
                fakeNotification.Status = ProgressNotificationStatus.InProgress;
                fakeNotification2.CurrentStatus = DateTime.UtcNow.ToString();
                fakeNotification.CurrentStatus = DateTime.Now.ToString();
                return new List<ProgressNotification> { fakeNotification  };
            }
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