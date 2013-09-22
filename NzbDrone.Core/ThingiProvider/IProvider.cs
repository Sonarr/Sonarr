
using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProviderRepository<TProvider> : IBasicRepository<TProvider> where TProvider : ModelBase, new()
    {
        TProvider GetByName(string name);
    }


    public class ProviderRepository<TProviderDefinition> : BasicRepository<TProviderDefinition>, IProviderRepository<TProviderDefinition>
        where TProviderDefinition : ModelBase,
        new()
    {
        protected ProviderRepository(IDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public TProviderDefinition GetByName(string name)
        {
            throw new NotImplementedException();
        }
    }

    public interface IProvider
    {
        Type ConfigContract { get; }

        IEnumerable<ProviderDefinition> DefaultDefinitions { get; }
        ProviderDefinition Definition { get; set; }
    }

    public abstract class ProviderDefinition : ModelBase
    {
        private IProviderConfig _settings;
        public string Name { get; set; }
        public string Implementation { get; set; }
        public bool Enable { get; set; }

        public string ConfigContract { get; set; }

        public IProviderConfig Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
                if (value != null)
                {
                    ConfigContract = value.GetType().Name;
                }
            }
        }
    }

    public interface IProviderConfig
    {
        ValidationResult Validate();
    }

    public class NullConfig : IProviderConfig
    {
        public static readonly NullConfig Instance = new NullConfig();

        public ValidationResult Validate()
        {
            return new ValidationResult();
        }
    }
}