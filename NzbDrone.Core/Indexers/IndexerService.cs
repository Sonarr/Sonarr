using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Lifecycle;

namespace NzbDrone.Core.Indexers
{
    public class Indexer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Enable { get; set; }
        public IIndexerSetting Settings { get; set; }
        public IIndexer Instance { get; set; }
        public string Implementation { get; set; }
    }

    public interface IIndexerService
    {
        List<Indexer> All();
        List<IIndexer> GetAvailableIndexers();
        Indexer Get(string name);
        List<Indexer> Schema();
        Indexer Create(Indexer indexer);
    }

    public class IndexerService : IIndexerService, IHandle<ApplicationStartedEvent>
    {
        private readonly IIndexerRepository _indexerRepository;
        private readonly Logger _logger;

        private readonly List<IIndexer> _indexers;

        public IndexerService(IIndexerRepository indexerRepository, IEnumerable<IIndexer> indexers, Logger logger)
        {
            _indexerRepository = indexerRepository;
            _logger = logger;
            _indexers = indexers.ToList();
        }

        public List<Indexer> All()
        {
            return _indexerRepository.All().Select(ToIndexer).ToList();
        }

        public List<IIndexer> GetAvailableIndexers()
        {
            return All().Where(c => c.Enable && c.Settings.IsValid).Select(c => c.Instance).ToList();
        }

        public Indexer Get(string name)
        {
            return ToIndexer(_indexerRepository.Get(name));
        }

        public List<Indexer> Schema()
        {
            var indexers = new List<Indexer>();

            var newznab = new Indexer();
            newznab.Instance = new Newznab.Newznab();
            newznab.Id = 1;
            newznab.Name = "Newznab";
            newznab.Settings = new NewznabSettings();
            newznab.Implementation = "Newznab";

            indexers.Add(newznab);

            return indexers;
        }

        public Indexer Create(Indexer indexer)
        {
            var definition = new IndexerDefinition
                                 {
                                     Name = indexer.Name,
                                     Enable = indexer.Enable,
                                     Implementation = indexer.Implementation,
                                     Settings = Json.Serialize(indexer.Settings)
                                 };

            definition = _indexerRepository.Insert(definition);
            indexer.Id = definition.Id;

            return indexer;
        }

        private Indexer ToIndexer(IndexerDefinition definition)
        {
            var indexer = new Indexer();
            indexer.Id = definition.Id;
            indexer.Enable = definition.Enable;
            indexer.Instance = GetInstance(definition);
            indexer.Name = definition.Name;
            indexer.Implementation = definition.Implementation;

            if (indexer.Instance.GetType().GetMethod("ImportSettingsFromJson") != null)
            {
                indexer.Settings = ((dynamic)indexer.Instance).ImportSettingsFromJson(definition.Settings);
            }
            else
            {
                indexer.Settings = NullSetting.Instance;
            }

            return indexer;
        }

        private IIndexer GetInstance(IndexerDefinition indexerDefinition)
        {
            var type = _indexers.Single(c => c.GetType().Name.Equals(indexerDefinition.Implementation, StringComparison.InvariantCultureIgnoreCase)).GetType();

            var instance = (IIndexer)Activator.CreateInstance(type);

            instance.InstanceDefinition = indexerDefinition;
            return instance;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _logger.Debug("Initializing indexers. Count {0}", _indexers.Count);

            if (!All().Any())
            {
                var definitions = _indexers.SelectMany(indexer => indexer.DefaultDefinitions);
                _indexerRepository.InsertMany(definitions.ToList());
            }
        }
    }
}