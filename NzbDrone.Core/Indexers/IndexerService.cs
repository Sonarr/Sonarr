using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Indexers.Providers;
using PetaPoco;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerService
    {
        List<Indexer> All();
        List<IndexerBase> GetEnabledIndexers();
        void SaveSettings(Indexer indexer);
        Indexer GetSettings(Type type);
    }

    public class IndexerService : IIndexerService
    {
        private readonly IIndexerRepository _indexerRepository;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IList<IndexerBase> _indexers;

        public IndexerService(IIndexerRepository indexerRepository, IEnumerable<IndexerBase> indexers)
        {
            _indexerRepository = indexerRepository;
            _indexers = indexers.ToList();
            InitializeIndexers();
        }

        public List<Indexer> All()
        {
            return _indexerRepository.All().ToList();
        }

        public List<IndexerBase> GetEnabledIndexers()
        {
            var all = All();
            return _indexers.Where(i => all.Exists(c => c.Type == i.GetType().ToString() && c.Enable)).ToList();
        }

        public void SaveSettings(Indexer indexer)
        {
            if (indexer.OID == 0)
            {
                Logger.Debug("Adding Indexer definitions for {0}", indexer.Name);
                _indexerRepository.Insert(indexer);
            }
            else
            {
                Logger.Debug("Updating Indexer definitions for {0}", indexer.Name);
                _indexerRepository.Update(indexer);
            }
        }

        public Indexer GetSettings(Type type)
        {
            return _indexerRepository.Find(type);
        }

        private void InitializeIndexers()
        {
            Logger.Debug("Initializing indexers. Count {0}", _indexers.Count);


            var currentIndexers = All();

            foreach (var feedProvider in _indexers)
            {
                IndexerBase indexerLocal = feedProvider;
                if (!currentIndexers.Exists(c => c.Type == indexerLocal.GetType().ToString()))
                {
                    var settings = new Indexer
                                       {
                                           Enable = indexerLocal.EnabledByDefault,
                                           Type = indexerLocal.GetType().ToString(),
                                           Name = indexerLocal.Name
                                       };

                    SaveSettings(settings);
                }
            }
        }
    }
}