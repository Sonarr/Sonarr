using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public interface IQualityProfileRepository : IBasicRepository<QualityProfile>
    {
        
    }

    public class QualityProfileRepository : BasicRepository<QualityProfile>, IQualityProfileRepository
    {
        public QualityProfileRepository(IDatabase database)
                : base(database)
        {
        }
    }
}
