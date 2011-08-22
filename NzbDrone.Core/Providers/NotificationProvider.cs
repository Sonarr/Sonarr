using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers
{
    public class NotificationProvider
    {

        private static readonly Object _lock = new object();

        private static readonly Dictionary<Guid, ProgressNotification> _progressNotification =
            new Dictionary<Guid, ProgressNotification>();

        public virtual List<ProgressNotification> ProgressNotifications
        {
            get
            {
                lock (_lock)
                {
                    var activeNotification =
                        _progressNotification.Values.Where(p => p.Status == ProgressNotificationStatus.InProgress).
                            ToList();

                    if (activeNotification.Count == 0)
                    {
                        //Get notifications that were recently done
                        activeNotification =
                            _progressNotification.Values.Where(p => p.CompletedTime >= DateTime.Now.AddSeconds(-3)).
                                OrderByDescending(c => c.CompletedTime).ToList();

                    }

                    return activeNotification.ToList();
                }
            }
        }

        public virtual void Register(ProgressNotification notification)
        {
            lock (_lock)
            {
                _progressNotification.Add(notification.Id, notification);
            }
        }
    }
}