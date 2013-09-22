
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
        string Name { get; }

        IEnumerable<ProviderDefinition> DefaultDefinitions { get; }
        ProviderDefinition Definition { get; set; }
    }

    public abstract class ProviderDefinition : ModelBase
    {
        public string Name { get; set; }
        public string Implementation { get; set; }
        public bool Enable { get; set; }

        public string ConfigContract
        {
            get
            {
                if (Settings == null) return null;
                return Settings.GetType().Name;
            }
            set
            {

            }
        }

        public IProviderConfig Settings { get; set; }
    }

    public interface IProviderConfig
    {
        ValidationResult Validate();
    }

    public class NullSetting : IProviderConfig
    {
        public static readonly NullSetting Instance = new NullSetting();

        public ValidationResult Validate()
        {
            return new ValidationResult();
        }
    }
}