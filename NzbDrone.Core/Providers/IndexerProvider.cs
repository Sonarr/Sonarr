using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using SubSonic.Repository;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class IndexerProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _sonicRepo;
        private readonly ConfigProvider _configProvider;

        public IndexerProvider(IRepository sonicRepo, ConfigProvider configProvider)
        {
            _sonicRepo = sonicRepo;
            _configProvider = configProvider;
        }

        #region IndexerProvider Members

        public virtual List<Indexer> AllIndexers()
        {
            return _sonicRepo.All<Indexer>().OrderBy(i => i.Order).ToList();
        }

        public virtual List<Indexer> EnabledIndexers()
        {
            return _sonicRepo.All<Indexer>().Where(i => i.Enabled).OrderBy(i => i.Order).ToList();
        }

        public virtual void Update(Indexer indexer)
        {
            _sonicRepo.Update(indexer);
        }

        public virtual Indexer Single(int indexerId)
        {
            return _sonicRepo.Single<Indexer>(indexerId);
        }

        #endregion
    }
}
