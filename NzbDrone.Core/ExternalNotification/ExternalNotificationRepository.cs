using System.Data;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ExternalNotification
{
    public interface IExternalNotificationRepository : IBasicRepository<ExternalNotificationDefinition>
    {
        ExternalNotificationDefinition Get(string name);
    }

    public class ExternalNotificationRepository : BasicRepository<ExternalNotificationDefinition>, IExternalNotificationRepository
    {
        public ExternalNotificationRepository(IDatabase database)
            : base(database)
        {
        }
        
        public ExternalNotificationDefinition Get(string name)
        {
            return Query.SingleOrDefault(c => c.Name.ToLower() == name.ToLower());
        }
    }
}