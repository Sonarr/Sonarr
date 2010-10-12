using System;
using System.Collections.Generic;
using NzbDrone.Core.Entities.Notification;

namespace NzbDrone.Core.Providers
{
    public interface INotificationProvider
    {
        void Register(ProgressNotification notification);
        void Register(BasicNotification notification);
        
        List<BasicNotification> BasicNotifications { get; }
        List<ProgressNotification> GetProgressNotifications { get; }

        /// <summary>
        /// Dismisses a notification based on its ID.
        /// </summary>
        /// <param name="notificationId">notification id.</param>
        void Dismiss(Guid notificationId);

    }
}
