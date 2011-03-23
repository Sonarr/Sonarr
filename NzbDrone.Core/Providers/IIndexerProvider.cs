using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public interface IIndexerProvider
    {
        List<Indexer> AllIndexers();
        List<Indexer> EnabledIndexers();
        void Update(Indexer indexer);
        Indexer Single(int indexerId);
    }
}
