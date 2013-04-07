using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Indexers.Newznab
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
            return Query.Where(n => n.Enable);
        }
    }
}
