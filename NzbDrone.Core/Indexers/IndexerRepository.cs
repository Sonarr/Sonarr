using System;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Indexers
{
    public interface IIndexerRepository : IBasicRepository<IndexerDefinition>
    {
        IndexerDefinition Get(string name);
        IndexerDefinition Find(string name);
    }

    public class IndexerRepository : BasicRepository<IndexerDefinition>, IIndexerRepository
    {
        public IndexerRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public IndexerDefinition Get(string name)
        {
            return Query.Single(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public IndexerDefinition Find(string name)
        {
            return Query.SingleOrDefault(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

    }
}
