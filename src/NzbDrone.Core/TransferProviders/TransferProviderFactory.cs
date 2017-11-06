using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.TransferProviders
{
    public class TransferProviderDefinition : ProviderDefinition
    {
    }

    public interface ITransferProviderFactory : IProviderFactory<ITransferProvider, TransferProviderDefinition>
    {
    }

    public abstract class TransferProviderBase<TSettings> : ITransferProvider where TSettings : IProviderConfig, new()
    {
        public abstract string Name { get; }

        public Type ConfigContract => typeof(TSettings);

        public virtual ProviderMessage Message => null;

        public IEnumerable<ProviderDefinition> DefaultDefinitions => new List<ProviderDefinition>();
        public ProviderDefinition Definition { get; set; }
        public abstract ValidationResult Test();

        public abstract string Link { get; }

        public virtual object RequestAction(string action, IDictionary<string, string> query) { return null; }
    }

    public interface ITransferProviderRepository : IProviderRepository<TransferProviderDefinition>
    {

    }

    public class TransferProviderFactory : ProviderFactory<ITransferProvider, TransferProviderDefinition>, ITransferProviderFactory
    {
        public TransferProviderFactory(ITransferProviderRepository providerRepository, IEnumerable<ITransferProvider> providers, IContainer container, IEventAggregator eventAggregator, Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
        }
    }
}
