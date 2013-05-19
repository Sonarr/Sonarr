using System;
using System.Linq;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Notifications
{
    public interface INotificationRepository : IBasicRepository<NotificationDefinition>
    {
        NotificationDefinition Get(string name);
        NotificationDefinition Find(string name);
    }

    public class NotificationRepository : BasicRepository<NotificationDefinition>, INotificationRepository
    {
        public NotificationRepository(IDatabase database, IMessageAggregator messageAggregator)
            : base(database, messageAggregator)
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