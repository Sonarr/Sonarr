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
    public interface ITransferProviderFactory : IProviderFactory<ITransferProvider, TransferProviderDefinition>
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
