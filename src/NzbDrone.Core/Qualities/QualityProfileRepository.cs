using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Qualities
{
    public interface IQualityProfileRepository : IBasicRepository<QualityProfile>
    {
        
    }

    public class QualityProfileRepository : BasicRepository<QualityProfile>, IQualityProfileRepository
    {
        public QualityProfileRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
