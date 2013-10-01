using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ThingiProvider
{
    public class ProviderRepository<TProviderDefinition> : BasicRepository<TProviderDefinition>, IProviderRepository<TProviderDefinition>
        where TProviderDefinition : ModelBase,
            new()
    {
        protected ProviderRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}