using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public interface IRootDirProvider
    {
        List<RootDir> GetAll();
        void Add(RootDir rootDir);
        void Remove(int rootDirId);
        void Update(RootDir rootDir);
    }
}
