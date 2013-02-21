using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerRepository : IBasicRepository<Indexer>
    {
        Indexer Find(Type type);
    }

    public class IndexerRepository : BasicRepository<Indexer>, IIndexerRepository
    {
        public IndexerRepository(IObjectDatabase objectDatabase)
            : base(objectDatabase)
        {
        }

        public Indexer Find(Type type)
        {
            return Queryable.Single(i => i.Type == type.ToString());
        }
    }
}
