using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Repository;
using System.Linq;

namespace NzbDrone.Core.RootFolders
{
    public interface IRootFolderRepository
    {
        List<RootDir> All();
        RootDir Get(int rootFolderId);
        RootDir Add(RootDir rootFolder);
        void Delete(int rootFolderId);
    }

    public class RootFolderRepository : IRootFolderRepository
    {
        private readonly EloqueraDb _db;

        public RootFolderRepository(EloqueraDb db)
        {
            _db = db;
        }

        public List<RootDir> All()
        {
            return _db.AsQueryable<RootDir>().ToList();
        }

        public RootDir Get(int rootFolderId)
        {
            return _db.AsQueryable<RootDir>().Single(c => c.Id == rootFolderId);
        }

        public RootDir Add(RootDir rootFolder)
        {
            return _db.Insert(rootFolder);
        }

        public void Delete(int rootFolderId)
        {
            _db.Delete(Get(rootFolderId));
        }
    }

}
