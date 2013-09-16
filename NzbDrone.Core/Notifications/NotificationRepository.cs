using System;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Notifications
{
    public interface INotificationRepository : IBasicRepository<NotificationDefinition>
    {
        NotificationDefinition Get(string name);
        NotificationDefinition Find(string name);
    }

    public class NotificationRepository : BasicRepository<NotificationDefinition>, INotificationRepository
    {
        public NotificationRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public NotificationDefinition Get(string name)
        {
            return Query.Single(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public NotificationDefinition Find(string name)
        {
            return Query.SingleOrDefault(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}