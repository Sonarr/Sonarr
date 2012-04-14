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

        private IEnumerable<IndexerBase> _indexers;

        [Inject]
        public IndexerProvider(IDatabase database, IEnumerable<IndexerBase> indexers)
        {
            _database = database;
            _indexers = indexers;
        }

        public IndexerProvider()
        {

        }

        public virtual IList<IndexerBase> GetEnabledIndexers()
        {
            var all = All();
            return _indexers.Where(i => all.Exists(c => c.IndexProviderType == i.GetType().ToString() && c.Enable)).ToList();
        }

        public virtual List<IndexerDefinition> All()
        {
            return _database.Fetch<IndexerDefinition>();
        }

        public virtual void SaveSettings(IndexerDefinition definitions)
        {
            if (definitions.Id == 0)
            {
                Logger.Debug("Adding Indexer definitions for {0}", definitions.Name);
                _database.Insert(definitions);
            }
            else
            {
                Logger.Debug("Updating Indexer definitions for {0}", definitions.Name);
                _database.Update(definitions);
            }
        }

        public virtual IndexerDefinition GetSettings(Type type)
        {
            return _database.Single<IndexerDefinition>("WHERE IndexProviderType = @0", type.ToString());
        }

        public virtual void InitializeIndexers(IList<IndexerBase> indexers)
        {
            Logger.Debug("Initializing indexers. Count {0}", indexers.Count);

            _indexers = indexers;

            var currentIndexers = All();

            foreach (var feedProvider in indexers)
            {
                IndexerBase indexerLocal = feedProvider;
                if (!currentIndexers.Exists(c => c.IndexProviderType == indexerLocal.GetType().ToString()))
                {
                    var settings = new IndexerDefinition
                                       {
                                           Enable = indexerLocal.EnabledByDefault,
                                           IndexProviderType = indexerLocal.GetType().ToString(),
                                           Name = indexerLocal.Name
                                       };

                    SaveSettings(settings);
                }
            }
        }
    }
}