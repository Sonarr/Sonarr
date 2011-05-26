using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class IndexerProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _repository;

        public IndexerProvider(IRepository repository)
        {
            _repository = repository;
        }

        public IndexerProvider()
        {

        }

        public virtual List<IndexerSetting> All()
        {
            return _repository.All<IndexerSetting>().ToList();
        }

        public virtual void SaveSettings(IndexerSetting settings)
        {
            if (settings.Id == 0)
            {
                Logger.Debug("Adding Indexer settings for {0}", settings.Name);
                _repository.Add(settings);
            }
            else
            {
                Logger.Debug("Updating Indexer settings for {0}", settings.Name);
                _repository.Update(settings);
            }
        }

        public virtual IndexerSetting GetSettings(Type type)
        {
            return _repository.Single<IndexerSetting>(s => s.IndexProviderType == type.ToString());
        }

        public virtual IndexerSetting GetSettings(int id)
        {
            return _repository.Single<IndexerSetting>(s => s.Id == id);
        }

        public virtual void InitializeIndexers(IList<IndexerBase> indexers)
        {
            Logger.Info("Initializing indexers. Count {0}", indexers.Count);

            var currentIndexers = All();

            foreach (var feedProvider in indexers)
            {
                IndexerBase indexerLocal = feedProvider;
                if (!currentIndexers.Exists(c => c.IndexProviderType == indexerLocal.GetType().ToString()))
                {
                    var settings = new IndexerSetting()
                                       {
                                           Enable = false,
                                           IndexProviderType = indexerLocal.GetType().ToString(),
                                           Name = indexerLocal.Name
                                       };

                    SaveSettings(settings);
                }
            }
        }
    }
}