using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Core.Download
{
    public interface IProvideDownloadClient
    {
        IDownloadClient GetDownloadClient(DownloadProtocol downloadProtocol, int indexerId = 0, bool filterBlockedClients = false, HashSet<int> tags = null);
        IEnumerable<IDownloadClient> GetDownloadClients(bool filterBlockedClients = false);
        IDownloadClient Get(int id);
    }

    public class DownloadClientProvider : IProvideDownloadClient
    {
        private readonly Logger _logger;
        private readonly IDownloadClientFactory _downloadClientFactory;
        private readonly IDownloadClientStatusService _downloadClientStatusService;
        private readonly IIndexerFactory _indexerFactory;
        private readonly ICached<int> _lastUsedDownloadClient;

        public DownloadClientProvider(IDownloadClientStatusService downloadClientStatusService,
                                      IDownloadClientFactory downloadClientFactory,
                                      IIndexerFactory indexerFactory,
                                      ICacheManager cacheManager,
                                      Logger logger)
        {
            _logger = logger;
            _downloadClientFactory = downloadClientFactory;
            _downloadClientStatusService = downloadClientStatusService;
            _indexerFactory = indexerFactory;
            _lastUsedDownloadClient = cacheManager.GetCache<int>(GetType(), "lastDownloadClientId");
        }

        public IDownloadClient GetDownloadClient(DownloadProtocol downloadProtocol, int indexerId = 0, bool filterBlockedClients = false, HashSet<int> tags = null)
        {
            var blockedProviders = new HashSet<int>(_downloadClientStatusService.GetBlockedProviders().Select(v => v.ProviderId));
            var availableProviders = _downloadClientFactory.GetAvailableProviders().Where(v => v.Protocol == downloadProtocol).ToList();

            if (tags != null)
            {
                var matchingTagsClients = availableProviders.Where(i => i.Definition.Tags.Intersect(tags).Any()).ToList();

                availableProviders = matchingTagsClients.Count > 0 ?
                    matchingTagsClients :
                    availableProviders.Where(i => i.Definition.Tags.Empty()).ToList();
            }

            if (!availableProviders.Any())
            {
                return null;
            }

            if (indexerId > 0)
            {
                var indexer = _indexerFactory.Find(indexerId);

                if (indexer != null && indexer.DownloadClientId > 0)
                {
                    var client = availableProviders.SingleOrDefault(d => d.Definition.Id == indexer.DownloadClientId);

                    if (client == null || (filterBlockedClients && blockedProviders.Contains(client.Definition.Id)))
                    {
                        throw new DownloadClientUnavailableException($"Indexer specified download client is not available");
                    }

                    return client;
                }
            }

            if (blockedProviders.Any())
            {
                var nonBlockedProviders = availableProviders.Where(v => !blockedProviders.Contains(v.Definition.Id)).ToList();

                if (nonBlockedProviders.Any())
                {
                    availableProviders = nonBlockedProviders;
                }
                else if (filterBlockedClients)
                {
                    throw new DownloadClientUnavailableException($"All download clients for {downloadProtocol} are not available");
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

        public IEnumerable<IDownloadClient> GetDownloadClients(bool filterBlockedClients = false)
        {
            var enabledClients = _downloadClientFactory.GetAvailableProviders();

            if (filterBlockedClients)
            {
                return FilterBlockedDownloadClients(enabledClients).ToList();
            }

            return enabledClients;
        }

        public IDownloadClient Get(int id)
        {
            return _downloadClientFactory.GetAvailableProviders().Single(d => d.Definition.Id == id);
        }

        private IEnumerable<IDownloadClient> FilterBlockedDownloadClients(IEnumerable<IDownloadClient> clients)
        {
            var blockedClients = _downloadClientStatusService.GetBlockedProviders().ToDictionary(v => v.ProviderId, v => v);

            foreach (var client in clients)
            {
                if (blockedClients.TryGetValue(client.Definition.Id, out var blockedClientStatus))
                {
                    _logger.Debug("Temporarily ignoring client {0} till {1} due to recent failures.", client.Definition.Name, blockedClientStatus.DisabledTill.Value.ToLocalTime());
                    continue;
                }

                yield return client;
            }
        }
    }
}
