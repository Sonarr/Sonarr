using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Indexers
{
    public interface INewznabRepository : IBasicRepository<NewznabDefinition>
    {
        IEnumerable<NewznabDefinition> Enabled();
    }

    public class NewznabRepository : BasicRepository<NewznabDefinition>, INewznabRepository
    {
        public NewznabRepository(IDatabase database)
            : base(database)
        {
        }

        public IEnumerable<NewznabDefinition> Enabled()
        {
            return Queryable().Where(n => n.Enable);
        }
    }
}
