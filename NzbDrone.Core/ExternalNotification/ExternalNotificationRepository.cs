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
        public ExternalNotificationRepository(IObjectDatabase objectDatabase)
            : base(objectDatabase)
        {
        }
        
        public ExternalNotificationDefinition Get(string name)
        {
            return Queryable.SingleOrDefault(c => c.Name.ToLower() == name.ToLower());
        }
    }
}