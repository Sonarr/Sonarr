using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ThingiProvider
{
    public class ProviderRepository<TProviderDefinition> : BasicRepository<TProviderDefinition>, IProviderRepository<TProviderDefinition>
        where TProviderDefinition : ModelBase,
            new()
    {
        protected ProviderRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

//        public void DeleteImplementations(string implementation)
//        {
//            DataMapper.Delete<TProviderDefinition>(c => c.Implementation == implementation);
//        }
    }
}
