using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Providers
{
    public interface IHistoryProvider
    {
        List<History> AllItems();
        void Purge();
        void Trim();
        void Insert(History item);
        bool Exists(int episodeId, QualityTypes quality, bool proper);
    }
}
