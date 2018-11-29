using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles.Qualities
{
    public interface IProfileRepository : IBasicRepository<QualityProfile>
    {
        bool Exists(int id);
    }

    public class QualityProfileRepository : BasicRepository<QualityProfile>, IProfileRepository
    {
        public QualityProfileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool Exists(int id)
        {
            return DataMapper.Query<QualityProfile>().Where(p => p.Id == id).GetRowCount() == 1;
        }
    }
}
