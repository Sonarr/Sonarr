using NzbDrone.Core.Datastore;
using System.Linq;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderRepository : IBasicRepository<RootFolder>
    {

    }
    
    public class RootFolderRepository : BasicRepository<RootFolder>, IRootFolderRepository
    {
        public RootFolderRepository(IObjectDatabase objectDatabase)
            : base(objectDatabase)
        {

        }
    }
}
