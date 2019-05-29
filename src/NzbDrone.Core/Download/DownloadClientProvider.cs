using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Indexers;
using NzbDrone.Common.Cache;
using NLog;

namespace NzbDrone.Core.Download
{
    public interface IProvideDownloadClient
    {
        IDownloadClient GetDownloadClient(DownloadProtocol downloadProtocol);
        IEnumerable<IDownloadClient> GetDownloadClients();
        IDownloadClient Get(int id);
    }

    public class DownloadClientProvider : IProvideDownloadClient
    {
        private readonly Logger _logger;
        private readonly IDownloadClientFactory _downloadClientFactory;
        private readonly IDownloadClientStatusService _downloadClientStatusService;
        private readonly ICached<int> _lastUsedDownloadClient;

        public DownloadClientProvider(IDownloadClientStatusService downloadClientStatusService, IDownloadClientFactory downloadClientFactory, ICacheManager cacheManager, Logger logger)
        {
            _logger = logger;
            _downloadClientFactory = downloadClientFactory;
            _downloadClientStatusService = downloadClientStatusService;
            _lastUsedDownloadClient = cacheManager.GetCache<int>(GetType(), "lastDownloadClientId");
        }

        public IDownloadClient GetDownloadClient(DownloadProtocol downloadProtocol)
        {
            var availableProviders = _downloadClientFactory.GetAvailableProviders().Where(v => v.Protocol == downloadProtocol).ToList();

            if (!availableProviders.Any()) return null;

            var blockedProviders = new HashSet<int>(_downloadClientStatusService.GetBlockedProviders().Select(v => v.ProviderId));

            if (blockedProviders.Any())
            {
                var nonBlockedProviders = availableProviders.Where(v => !blockedProviders.Contains(v.Definition.Id)).ToList();

                if (nonBlockedProviders.Any())
                {
                    availableProviders = nonBlockedProviders;
                }
                else
                {
                    _logger.Trace("No non-blocked Download Client available, retrying blocked one.");
                }
            }

            // Use the first priority clients first
            availableProviders = availableProviders.GroupBy(v => (v.Definition as DownloadClientDefinition).Priority)
                                                   .OrderBy(v => v.Key)
                                                   .First().OrderBy(v => v.Definition.Id).ToList();

            var lastId = _lastUsedDownloadClient.Find(downloadProtocol.ToString());

            var provider = availableProviders.FirstOrDefault(v => v.Definition.Id > lastId) ?? availableProviders.First();

            _lastUsedDownloadClient.Set(downloadProtocol.ToString(), provider.Definition.Id);

            return provider;
        }

        public IEnumerable<IDownloadClient> GetDownloadClients()
        {
            return _downloadClientFactory.GetAvailableProviders();
        }

        public IDownloadClient Get(int id)
        {
            return _downloadClientFactory.GetAvailableProviders().Single(d => d.Definition.Id == id);
        }
    }
}
