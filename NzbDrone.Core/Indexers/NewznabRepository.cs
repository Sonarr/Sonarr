using System;
using System.Collections.Generic;
using System.Data;
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
        public NewznabRepository(IDbConnection database) : base(database)
        {
        }

        public IEnumerable<NewznabDefinition> Enabled()
        {
            return Where(n => n.Enabled);
        }
    }
}
