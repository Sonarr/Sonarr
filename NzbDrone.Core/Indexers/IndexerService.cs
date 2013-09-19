using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using Omu.ValueInjecter;

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
        Indexer Get(int id);
        Indexer Get(string name);
        List<Indexer> Schema();
        Indexer Create(Indexer indexer);
        Indexer Update(Indexer indexer);
        void Delete(int id);
    }

    public class IndexerService : IIndexerService, IHandle<ApplicationStartedEvent>
    {
        private readonly IIndexerRepository _indexerRepository;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        private readonly List<IIndexer> _indexers;

        public IndexerService(IIndexerRepository indexerRepository, IEnumerable<IIndexer> indexers, IConfigFileProvider configFileProvider, Logger logger)
        {
            _indexerRepository = indexerRepository;
            _configFileProvider = configFileProvider;
            _logger = logger;


            if (!configFileProvider.Torrent)
            {
                _indexers = indexers.Where(c => c.Kind != IndexerKind.Torrent).ToList();
            }
            else
            {
                _indexers = indexers.ToList();
            }
        }

        public List<Indexer> All()
        {
            return _indexerRepository.All().Select(ToIndexer).ToList();
        }

        public List<IIndexer> GetAvailableIndexers()
        {
            return All().Where(c => c.Enable && c.Settings.Validate().IsValid).Select(c => c.Instance).ToList();
        }

        public Indexer Get(int id)
        {
            return ToIndexer(_indexerRepository.Get(id));
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
                                     Settings = indexer.Settings.ToJson()
                                 };

            definition = _indexerRepository.Insert(definition);
            indexer.Id = definition.Id;

            return indexer;
        }

        public Indexer Update(Indexer indexer)
        {
            var definition = _indexerRepository.Get(indexer.Id);
            definition.InjectFrom(indexer);
            definition.Settings = indexer.Settings.ToJson();
            _indexerRepository.Update(definition);

            return indexer;
        }

        public void Delete(int id)
        {
            _indexerRepository.Delete(id);
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
            var type = GetImplementation(indexerDefinition);
            var instance = (IIndexer)Activator.CreateInstance(type);
            instance.InstanceDefinition = indexerDefinition;
            return instance;
        }

        private Type GetImplementation(IndexerDefinition indexerDefinition)
        {
            return _indexers.Select(c => c.GetType()).SingleOrDefault(c => c.Name.Equals(indexerDefinition.Implementation, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _logger.Debug("Initializing indexers. Count {0}", _indexers.Count);

            RemoveMissingImplementations();

            var definitions = _indexers.SelectMany(indexer => indexer.DefaultDefinitions);

            var currentIndexer = All();

            var newIndexers = definitions.Where(def => currentIndexer.All(c => c.Implementation != def.Implementation)).ToList();


            if (newIndexers.Any())
            {
                _indexerRepository.InsertMany(newIndexers);
            }
        }

        private void RemoveMissingImplementations()
        {
            var storedIndexers = _indexerRepository.All();

            foreach (var indexerDefinition in storedIndexers.Where(i => GetImplementation(i) == null))
            {
                _logger.Debug("Removing Indexer {0} ", indexerDefinition.Name);
                _indexerRepository.Delete(indexerDefinition);
            }
        }
    }
}