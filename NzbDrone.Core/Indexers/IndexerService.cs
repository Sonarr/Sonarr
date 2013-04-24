using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Lifecycle;


namespace NzbDrone.Core.Indexers
{
    public interface IIndexerService
    {
        List<IndexerDefinition> All();
        List<IIndexerBase> GetAvailableIndexers();
        void Save(IndexerDefinition indexer);
        IndexerDefinition Get(string name);
    }

    public class IndexerService : IIndexerService, IHandle<ApplicationStartedEvent>
    {
        private readonly IIndexerRepository _indexerRepository;
        private readonly Logger _logger;

        private readonly IList<IIndexerBase> _indexers;

        public IndexerService(IIndexerRepository indexerRepository, IEnumerable<IIndexerBase> indexers, Logger logger)
        {
            _indexerRepository = indexerRepository;
            _logger = logger;
            _indexers = indexers.ToList();
        }


        public List<IndexerDefinition> All()
        {
            return _indexerRepository.All().ToList();
        }

        public List<IIndexerBase> GetAvailableIndexers()
        {
            var enabled = All().Where(c => c.Enable).Select(c => c.Name);
            var configureIndexers = _indexers.Where(c => c.IsConfigured);

            return configureIndexers.Where(c => enabled.Contains(c.Name)).ToList();
        }

        public void Save(IndexerDefinition indexer)
        {
            _indexerRepository.Update(indexer);
        }

        public IndexerDefinition Get(string name)
        {
            return _indexerRepository.Get(name);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _logger.Debug("Initializing indexers. Count {0}", _indexers.Count);

            var currentIndexers = All();

            foreach (var feedProvider in _indexers)
            {
                IIndexerBase indexerLocal = feedProvider;
                if (!currentIndexers.Exists(c => c.Name.Equals(indexerLocal.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var settings = new IndexerDefinition
                    {
                        Enable = indexerLocal.EnabledByDefault,
                        Name = indexerLocal.Name.ToLower()
                    };

                    _indexerRepository.Insert(settings);
                }
            }
        }
    }
}