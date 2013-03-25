using System;
using System.Data;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerRepository : IBasicRepository<Indexer>
    {
        Indexer Find(Type type);
    }

    public class IndexerRepository : BasicRepository<Indexer>, IIndexerRepository
    {
        public IndexerRepository(IDatabase database)
            : base(database)
        {
        }

        public Indexer Find(Type type)
        {
            return Queryable().Single(i => i.Type == type.ToString());
        }
    }
}
