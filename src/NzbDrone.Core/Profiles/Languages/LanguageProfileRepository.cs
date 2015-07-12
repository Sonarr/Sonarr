using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles.Languages
{
    public interface ILanguageProfileRepository : IBasicRepository<LanguageProfile>
    {
        
    }

    public class LanguageProfileRepository : BasicRepository<LanguageProfile>, ILanguageProfileRepository
    {
        public LanguageProfileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
