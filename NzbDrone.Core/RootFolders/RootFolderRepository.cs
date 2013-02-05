using System.Collections.Generic;
using Eloquera.Client;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Repository;
using System.Linq;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderRepository : IBasicRepository<RootFolder>
    {

    }


    //This way we only need to implement none_custom methods for repos, like custom queries etc... rest is done automagically.
    public class RootFolderRepository : BasicRepository<RootFolder>, IRootFolderRepository
    {
        public RootFolderRepository(EloqueraDb eloqueraDb)
            : base(eloqueraDb)
        {
        }
    }

}
