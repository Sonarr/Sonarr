using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.ThingiProvider
{
    public abstract class ProviderFactory<TProvider, TProviderDefinition> : IProviderFactory<TProvider, TProviderDefinition>, IHandle<ApplicationStartedEvent>
        where TProviderDefinition : ProviderDefinition, new()
        where TProvider : IProvider
    {
        private readonly IProviderRepository<TProviderDefinition> _providerRepository;
        private readonly IServiceProvider _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        protected readonly List<TProvider> _providers;

        protected ProviderFactory(IProviderRepository<TProviderDefinition> providerRepository,
                                  IEnumerable<TProvider> providers,
                                  IServiceProvider container,
                                  IEventAggregator eventAggregator,
                                  Logger logger)
        {
            _providerRepository = providerRepository;
            _container = container;
            _eventAggregator = eventAggregator;
            _providers = providers.ToList();
            _logger = logger;
        }

        public List<TProviderDefinition> All()
        {
            return _providerRepository.All().ToList();
        }

        public IAsyncEnumerable<TProviderDefinition> AllAsync(CancellationToken cancellationToken = default)
        {
            return _providerRepository.AllAsync(cancellationToken);
        }

        public IEnumerable<TProviderDefinition> GetDefaultDefinitions()
        {
            foreach (var provider in _providers)
            {
                var definition = provider.DefaultDefinitions
                    .OfType<TProviderDefinition>()
                    .FirstOrDefault(v => v.Name == null || v.Name == provider.GetType().Name);

                if (definition == null)
                {
                    definition = new TProviderDefinition()
                    {
                        Name = string.Empty,
                        ConfigContract = provider.ConfigContract.Name,
                        Implementation = provider.GetType().Name,
                        Settings = (IProviderConfig)Activator.CreateInstance(provider.ConfigContract)
                    };
                }

                SetProviderCharacteristics(provider, definition);

                yield return definition;
            }
        }

        public IEnumerable<TProviderDefinition> GetPresetDefinitions(TProviderDefinition providerDefinition)
        {
            var provider = _providers.First(v => v.GetType().Name == providerDefinition.Implementation);

            var definitions = provider.DefaultDefinitions
                   .OfType<TProviderDefinition>()
                   .Where(v => v.Name != null && v.Name != provider.GetType().Name)
                   .ToList();

            return definitions;
        }

        public virtual ValidationResult Test(TProviderDefinition definition)
        {
            return GetInstance(definition).Test();
        }

        public object RequestAction(TProviderDefinition definition, string action, IDictionary<string, string> query)
        {
            return GetInstance(definition).RequestAction(action, query);
        }

        public List<TProvider> GetAvailableProviders()
        {
            return Active().Select(GetInstance).ToList();
        }

        public IAsyncEnumerable<TProvider> GetAvailableProvidersAsync(CancellationToken cancellationToken = default)
        {
            return ActiveAsync(cancellationToken).Select(GetInstance);
        }

        public bool Exists(int id)
        {
            return _providerRepository.Find(id) != null;
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _providerRepository.FindAsync(id, cancellationToken) is not null;
        }

        public TProviderDefinition Get(int id)
        {
            return _providerRepository.Get(id);
        }

        public async Task<TProviderDefinition> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _providerRepository.GetAsync(id, cancellationToken);
        }

        public IEnumerable<TProviderDefinition> Get(IEnumerable<int> ids)
        {
            return _providerRepository.Get(ids);
        }

        public IAsyncEnumerable<TProviderDefinition> GetAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            return _providerRepository.GetAsync(ids, cancellationToken);
        }

        public TProviderDefinition Find(int id)
        {
            return _providerRepository.Find(id);
        }

        public async Task<TProviderDefinition> FindAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _providerRepository.FindAsync(id, cancellationToken);
        }

        public virtual TProviderDefinition Create(TProviderDefinition definition)
        {
            var result = _providerRepository.Insert(definition);
            _eventAggregator.PublishEvent(new ProviderAddedEvent<TProvider>(result));

            return result;
        }

        public virtual async Task<TProviderDefinition> CreateAsync(TProviderDefinition definition, CancellationToken cancellationToken = default)
        {
            var result = await _providerRepository.InsertAsync(definition, cancellationToken);
            _eventAggregator.PublishEvent(new ProviderAddedEvent<TProvider>(result));

            return result;
        }

        public virtual void Update(TProviderDefinition definition)
        {
            _providerRepository.Update(definition);
            _eventAggregator.PublishEvent(new ProviderUpdatedEvent<TProvider>(definition));
        }

        public virtual async Task UpdateAsync(TProviderDefinition definition, CancellationToken cancellationToken = default)
        {
            await _providerRepository.UpdateAsync(definition, cancellationToken);
            _eventAggregator.PublishEvent(new ProviderUpdatedEvent<TProvider>(definition));
        }

        public virtual IEnumerable<TProviderDefinition> Update(IEnumerable<TProviderDefinition> definitions)
        {
            _providerRepository.UpdateMany(definitions.ToList());

            foreach (var definition in definitions)
            {
                _eventAggregator.PublishEvent(new ProviderUpdatedEvent<TProvider>(definition));
            }

            return definitions;
        }

        public virtual async Task<IEnumerable<TProviderDefinition>> UpdateAsync(IEnumerable<TProviderDefinition> definitions, CancellationToken cancellationToken = default)
        {
            await _providerRepository.UpdateManyAsync(definitions.ToList(), cancellationToken);

            foreach (var definition in definitions)
            {
                _eventAggregator.PublishEvent(new ProviderUpdatedEvent<TProvider>(definition));
            }

            return definitions;
        }

        public void Delete(int id)
        {
            _providerRepository.Delete(id);
            _eventAggregator.PublishEvent(new ProviderDeletedEvent<TProvider>(id));
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await _providerRepository.DeleteAsync(id, cancellationToken);
            _eventAggregator.PublishEvent(new ProviderDeletedEvent<TProvider>(id));
        }

        public void Delete(IEnumerable<int> ids)
        {
            _providerRepository.DeleteMany(ids);

            foreach (var id in ids)
            {
                _eventAggregator.PublishEvent(new ProviderDeletedEvent<TProvider>(id));
            }
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            await _providerRepository.DeleteManyAsync(ids, cancellationToken);

            foreach (var id in ids)
            {
                _eventAggregator.PublishEvent(new ProviderDeletedEvent<TProvider>(id));
            }
        }

        public TProvider GetInstance(TProviderDefinition definition)
        {
            var type = GetImplementation(definition);
            var instance = (TProvider)_container.GetRequiredService(type);
            instance.Definition = definition;
            SetProviderCharacteristics(instance, definition);
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

            InitializeProviders();
        }

        protected virtual void InitializeProviders()
        {
        }

        protected virtual List<TProviderDefinition> Active()
        {
            return All().Where(c => c.Settings.Validate().IsValid).ToList();
        }

        protected virtual IAsyncEnumerable<TProviderDefinition> ActiveAsync(CancellationToken cancellationToken = default)
        {
            return AllAsync(cancellationToken).Where(c => c.Settings.Validate().IsValid);
        }

        public void SetProviderCharacteristics(TProviderDefinition definition)
        {
            GetInstance(definition);
        }

        public virtual void SetProviderCharacteristics(TProvider provider, TProviderDefinition definition)
        {
            definition.ImplementationName = provider.Name;
            definition.Message = provider.Message;
        }

        private void RemoveMissingImplementations()
        {
            var storedProvider = _providerRepository.All();

            foreach (var invalidDefinition in storedProvider.Where(def => GetImplementation(def) == null))
            {
                _logger.Warn("Removing {0}", invalidDefinition.Name);
                _providerRepository.Delete(invalidDefinition);
            }
        }

        public List<TProviderDefinition> AllForTag(int tagId)
        {
            return All().Where(p => p.Tags.Contains(tagId))
                        .ToList();
        }

        public IAsyncEnumerable<TProviderDefinition> AllForTagAsync(int tagId, CancellationToken cancellationToken = default)
        {
            return AllAsync(cancellationToken).Where(p => p.Tags.Contains(tagId));
        }
    }
}
