using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class IndexerProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDatabase _database;

        private IList<IndexerBase> _indexers = new List<IndexerBase>();

        [Inject]
        public IndexerProvider(IDatabase database)
        {
            _database = database;
        }

        public IndexerProvider()
        {

        }

        public virtual IList<IndexerBase> GetEnabledIndexers()
        {
            var all = GetAllISettings();
            return _indexers.Where(i => all.Exists(c => c.IndexProviderType == i.GetType().ToString() && c.Enable)).ToList();
        }

        public virtual List<IndexerSetting> GetAllISettings()
        {
            return _database.Fetch<IndexerSetting>();
        }

        public virtual void SaveSettings(IndexerSetting settings)
        {
            if (settings.Id == 0)
            {
                Logger.Debug("Adding Indexer settings for {0}", settings.Name);
                _database.Insert(settings);
            }
            else
            {
                Logger.Debug("Updating Indexer settings for {0}", settings.Name);
                _database.Update(settings);
            }
        }

        public virtual IndexerSetting GetSettings(Type type)
        {
            return _database.Single<IndexerSetting>("WHERE IndexProviderType = @0", type.ToString());
        }

        public virtual void InitializeIndexers(IList<IndexerBase> indexers)
        {
            Logger.Info("Initializing indexers. Count {0}", indexers.Count);

            _indexers = indexers;

            var currentIndexers = GetAllISettings();

            foreach (var feedProvider in indexers)
            {
                IndexerBase indexerLocal = feedProvider;
                if (!currentIndexers.Exists(c => c.IndexProviderType == indexerLocal.GetType().ToString()))
                {
                    var settings = new IndexerSetting
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