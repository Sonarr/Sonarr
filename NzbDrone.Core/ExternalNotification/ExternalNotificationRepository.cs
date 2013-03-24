using System.Data;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ExternalNotification
{
    public interface IExternalNotificationRepository : IBasicRepository<ExternalNotificationDefinition>
    {
        ExternalNotificationDefinition Get(string name);
    }

    public class ExternalNotificationRepository : BasicRepository<ExternalNotificationDefinition>, IExternalNotificationRepository
    {
        public ExternalNotificationRepository(IDbConnection database)
            : base(database)
        {
        }
        
        public ExternalNotificationDefinition Get(string name)
        {
            return SingleOrDefault(c => c.Name.ToLower() == name.ToLower());
        }
    }
}