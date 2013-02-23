using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Indexers
{
    public interface INewznabRepository : IBasicRepository<NewznabDefinition>
    {
        IEnumerable<NewznabDefinition> Enabled();
    }

    public class NewznabRepository : BasicRepository<NewznabDefinition>, INewznabRepository
    {
        public NewznabRepository(IObjectDatabase objectDatabase) : base(objectDatabase)
        {
        }

        public IEnumerable<NewznabDefinition> Enabled()
        {
            return Queryable.Where(n => n.Enabled);
        }
    }
}
