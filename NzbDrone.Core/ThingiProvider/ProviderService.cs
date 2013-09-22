using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProviderFactory<TProvider, TProviderDefinition>
        where TProviderDefinition : ProviderDefinition, new()
        where TProvider : IProvider
    {
        List<TProviderDefinition> All();
        List<TProvider> GetAvailableProviders();
        TProviderDefinition Get(int id);
        //List<TProvider> Schema();
        TProviderDefinition Create(TProviderDefinition indexer);
        void Update(TProviderDefinition indexer);
        void Delete(int id);
    }

    public abstract class ProviderFactory<TProvider, TProviderDefinition> : IProviderFactory<TProvider, TProviderDefinition>, IHandle<ApplicationStartedEvent>
        where TProviderDefinition : ProviderDefinition, new()
        where TProvider : IProvider
    {
        private readonly IProviderRepository<TProviderDefinition> _providerRepository;
        private readonly Logger _logger;

        private readonly List<TProvider> _providers;

        protected ProviderFactory(IProviderRepository<TProviderDefinition> providerRepository, IEnumerable<TProvider> providers, Logger logger)
        {
            _providerRepository = providerRepository;
            _providers = providers.ToList();
            _logger = logger;
        }

        public List<TProviderDefinition> All()
        {
            return _providerRepository.All().ToList();
        }

        public List<TProvider> GetAvailableProviders()
        {
            return All().Where(c => c.Enable && c.Settings.Validate().IsValid)
                .Select(GetInstance).ToList();
        }

        public TProviderDefinition Get(int id)
        {
            return _providerRepository.Get(id);
        }

 /*       public List<TProvider> Schema()
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
        }*/

        public TProviderDefinition Create(TProviderDefinition provider)
        {
            return _providerRepository.Insert(provider);
        }

        public void Update(TProviderDefinition definition)
        {
            _providerRepository.Update(definition);
        }

        public void Delete(int id)
        {
            _providerRepository.Delete(id);
        }

        private TProvider GetInstance(TProviderDefinition definition)
        {
            var type = GetImplementation(definition);
            var instance = (TProvider)Activator.CreateInstance(type);
            instance.Definition = definition;
            return instance;
        }

        private Type GetImplementation(TProviderDefinition definition)
        {
            return _providers.Select(c => c.GetType()).SingleOrDefault(c => c.Name.Equals(definition.Implementation, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _logger.Debug("Initializing Providers. Count {0}", _providers.Count);

            RemoveMissingImplementations();

            var definitions = _providers.SelectMany(indexer => indexer.DefaultDefinitions);

            var currentProviders = All();

            var newProviders = definitions.Where(def => currentProviders.All(c => c.Implementation != def.Implementation)).ToList();


            if (newProviders.Any())
            {
                _providerRepository.InsertMany(newProviders.Cast<TProviderDefinition>().ToList());
            }
        }

        private void RemoveMissingImplementations()
        {
            var storedProvider = _providerRepository.All();

            foreach (var providerDefinition in storedProvider.Where(i => GetImplementation(i) == null))
            {
                _logger.Debug("Removing {0} ", providerDefinition.Name);
                _providerRepository.Delete(providerDefinition);
            }
        }
    }
}