using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class IndexerProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly IRepository _sonicRepo;

        public IndexerProvider(IRepository sonicRepo, ConfigProvider configProvider)
        {
            _sonicRepo = sonicRepo;
            _configProvider = configProvider;
        }

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
    }
}