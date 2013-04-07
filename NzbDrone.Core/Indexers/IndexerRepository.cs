using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerRepository : IBasicRepository<IndexerDefinition>
    {
        IndexerDefinition Get(string name);
    }

    public class IndexerRepository : BasicRepository<IndexerDefinition>, IIndexerRepository
    {
        public IndexerRepository(IDatabase database)
            : base(database)
        {
        }

        public IndexerDefinition Get(string name)
        {
            return Query.Single(i => i.Name.ToLower() == name.ToLower());
        }
    }
}
