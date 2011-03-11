using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class RootDirProvider : IRootDirProvider
    {
        private readonly IRepository _sonioRepo;

        public RootDirProvider(IRepository sonicRepo)
        {
            _sonioRepo = sonicRepo;
        }

        #region IRootDirProvider

        public List<RootDir> GetAll()
        {
            return _sonioRepo.All<RootDir>().ToList();
        }

        public void Add(RootDir rootDir)
        {
            _sonioRepo.Add(rootDir);
        }

        public void Remove(int rootDirId)
        {
            _sonioRepo.Delete<RootDir>(rootDirId);
        }

        public void Update(RootDir rootDir)
        {
            _sonioRepo.Update(rootDir);
        }

        public RootDir GetRootDir(int rootDirId)
        {
            return _sonioRepo.Single<RootDir>(rootDirId);
        }

        #endregion
    }
}
