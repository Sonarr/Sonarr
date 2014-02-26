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
        List<IDownloadClient> Enabled();
    }

    public class DownloadClientFactory : ProviderFactory<IDownloadClient, DownloadClientDefinition>, IDownloadClientFactory
    {
        private readonly IDownloadClientRepository _providerRepository;

        public DownloadClientFactory(IDownloadClientRepository providerRepository, IEnumerable<IDownloadClient> providers, IContainer container, IEventAggregator eventAggregator, Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
            _providerRepository = providerRepository;
        }

        public List<IDownloadClient> Enabled()
        {
            return GetAvailableProviders().Where(n => ((DownloadClientDefinition)n.Definition).Enable).ToList();
        }
    }
}