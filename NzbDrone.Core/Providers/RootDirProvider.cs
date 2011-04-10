using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class RootDirProvider
    {
        private readonly IRepository _sonioRepo;

        public RootDirProvider(IRepository sonicRepo)
        {
            _sonioRepo = sonicRepo;
        }

        #region IRootDirProvider

        public virtual List<RootDir> GetAll()
        {
            return _sonioRepo.All<RootDir>().ToList();
        }

        public virtual void Add(RootDir rootDir)
        {
            _sonioRepo.Add(rootDir);
        }

        public virtual void Remove(int rootDirId)
        {
            _sonioRepo.Delete<RootDir>(rootDirId);
        }

        public virtual void Update(RootDir rootDir)
        {
            _sonioRepo.Update(rootDir);
        }

        public virtual RootDir GetRootDir(int rootDirId)
        {
            return _sonioRepo.Single<RootDir>(rootDirId);
        }

        #endregion
    }
}