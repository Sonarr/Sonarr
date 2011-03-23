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
        private readonly IConfigProvider _configProvider;

        public IndexerProvider(IRepository sonicRepo, IConfigProvider configProvider)
        {
            _sonicRepo = sonicRepo;
            _configProvider = configProvider;
        }

        #region IIndexerProvider Members

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

        public Indexer Single(int indexerId)
        {
            return _sonicRepo.Single<Indexer>(indexerId);
        }

        #endregion
    }
}
