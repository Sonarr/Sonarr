using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public interface IDownloadClientFactory : IProviderFactory<IDownloadClient, DownloadClientDefinition>
    {

    }

    public class DownloadClientFactory : ProviderFactory<IDownloadClient, DownloadClientDefinition>, IDownloadClientFactory
    {
        private readonly IDownloadClientRepository _providerRepository;

        public DownloadClientFactory(IDownloadClientRepository providerRepository, IEnumerable<IDownloadClient> providers, IContainer container, IEventAggregator eventAggregator, Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
            _providerRepository = providerRepository;
        }

        protected override List<DownloadClientDefinition> Active()
        {
            return base.Active().Where(c => c.Enable).ToList();
        }

        public override DownloadClientDefinition GetProviderCharacteristics(IDownloadClient provider, DownloadClientDefinition definition)
        {
            definition = base.GetProviderCharacteristics(provider, definition);

            definition.Protocol = provider.Protocol;

            return definition;
        }
    }
}