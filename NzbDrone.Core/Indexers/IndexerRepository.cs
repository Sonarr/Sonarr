using System;
using System.Data;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerRepository : IBasicRepository<Indexer>
    {
        Indexer Find(Type type);
    }

    public class IndexerRepository : BasicRepository<Indexer>, IIndexerRepository
    {
        public IndexerRepository(IDbConnection database)
            : base(database)
        {
        }

        public Indexer Find(Type type)
        {
            return Single(i => i.Type == type.ToString());
        }
    }
}
