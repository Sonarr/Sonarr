using NzbDrone.Core.Datastore;
using System.Linq;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderRepository : IBasicRepository<RootFolder>
    {

    }

    //This way we only need to implement none_custom methods for repos, like custom queries etc... rest is done automagically.
    public class RootFolderRepository : BasicRepository<RootFolder>, IRootFolderRepository
    {
        public RootFolderRepository(IObjectDatabase objectDatabase)
            : base(objectDatabase)
        {

        }

        public RootFolder Add(RootFolder rootFolder)
        {
            return ObjectDatabase.Insert(rootFolder);
        }
    }
}
