using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Indexers.Providers;
using NzbDrone.Core.Lifecycle;
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

    public class IndexerService : IIndexerService, IInitializable
    {
        private readonly IIndexerRepository _indexerRepository;
        private readonly Logger _logger;

        private IList<IndexerBase> _indexers;

        public IndexerService(IIndexerRepository indexerRepository, IEnumerable<IndexerBase> indexers, Logger logger)
        {
            _indexerRepository = indexerRepository;
            _logger = logger;
            _indexers = indexers.ToList();
        }

        public void Init()
        {
            _logger.Debug("Initializing indexers. Count {0}", _indexers.Count);

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

                    _indexerRepository.Insert(settings);
                }
            }
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
            //Todo: This will be used in the API
            _logger.Debug("Upserting Indexer definitions for {0}", indexer.Name);
            _indexerRepository.Upsert(indexer);
        }

        public Indexer GetSettings(Type type)
        {
            return _indexerRepository.Find(type);
        }
    }
}