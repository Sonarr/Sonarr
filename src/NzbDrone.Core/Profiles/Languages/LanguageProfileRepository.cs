using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles.Languages
{
    public interface ILanguageProfileRepository : IBasicRepository<LanguageProfile>
    {
        bool Exists(int id);
    }

    public class LanguageProfileRepository : BasicRepository<LanguageProfile>, ILanguageProfileRepository
    {
        public LanguageProfileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool Exists(int id)
        {
            return Query(p => p.Id == id).Count == 1;
        }
    }
}
