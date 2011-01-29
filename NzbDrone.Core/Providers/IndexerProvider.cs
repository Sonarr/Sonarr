using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Model;
using SubSonic.Repository;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class IndexerProvider : IIndexerProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _sonicRepo;

        public IndexerProvider(IRepository sonicRepo)
        {
            _sonicRepo = sonicRepo;
        }

        #region IIndexerProvider

        public List<Indexer> AllIndexers()
        {
            return _sonicRepo.All<Indexer>().OrderBy(i => i.Order).ToList();
        }

        public List<Indexer> EnabledIndexers()
        {
            return _sonicRepo.All<Indexer>().Where(i => i.Enabled).OrderBy(i => i.Order).ToList();
        }

        public void Update(Indexer indexer)
        {
            _sonicRepo.Update(indexer);
        }

        #endregion
    }
}
