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
        IEnumerable<IDownloadClient> GetDownloadClients(DownloadProtocol downloadProtocol, int indexerId = 0, bool filterBlockedClients = false, HashSet<int> tags = null);
        IDownloadClient Get(int id);
        void ReportSuccessfulDownloadClient(DownloadProtocol downloadProtocol, int downloadClientId);
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
            var availableProviders = GetFilteredDownloadClients(downloadProtocol, indexerId, filterBlockedClients, tags, forSingleClient: true).ToList();

            if (!availableProviders.Any())
            {
                return null;
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

        public IEnumerable<IDownloadClient> GetDownloadClients(DownloadProtocol downloadProtocol, int indexerId = 0, bool filterBlockedClients = false, HashSet<int> tags = null)
        {
            var filteredClients = GetFilteredDownloadClients(downloadProtocol, indexerId, filterBlockedClients, tags, forSingleClient: false).ToList();

            if (filteredClients.Empty())
            {
                return Enumerable.Empty<IDownloadClient>();
            }

            var lastUsedId = _lastUsedDownloadClient.Find(downloadProtocol.ToString());

            var clientsByPriority = filteredClients
                .GroupBy(v => (v.Definition as DownloadClientDefinition).Priority)
                .OrderBy(g => g.Key)
                .ToList();

            var orderedClients = new List<IDownloadClient>();

            foreach (var priorityGroup in clientsByPriority)
            {
                var clientsInGroup = priorityGroup.OrderBy(v => v.Definition.Id).ToList();
                var lastUsedIndex = clientsInGroup.FindIndex(v => v.Definition.Id == lastUsedId);

                if (lastUsedIndex >= 0)
                {
                    orderedClients.AddRange(clientsInGroup.Skip(lastUsedIndex + 1));
                    orderedClients.AddRange(clientsInGroup.Take(lastUsedIndex));
                    orderedClients.Add(clientsInGroup[lastUsedIndex]);
                }
                else
                {
                    orderedClients.AddRange(clientsInGroup);
                }
            }

            return orderedClients;
        }

        public IDownloadClient Get(int id)
        {
            return _downloadClientFactory.GetAvailableProviders().Single(d => d.Definition.Id == id);
        }

        public void ReportSuccessfulDownloadClient(DownloadProtocol downloadProtocol, int downloadClientId)
        {
            _lastUsedDownloadClient.Set(downloadProtocol.ToString(), downloadClientId);
        }

        public DownloadClientDefinition ResolveDownloadClient(int? downloadClientId, string downloadClientName)
        {
            var all = _downloadClientFactory.All();
            var clientByName = downloadClientName.IsNullOrWhiteSpace() ? null : all.FirstOrDefault(c => c.Name.EqualsIgnoreCase(downloadClientName));
            var clientById = downloadClientId.HasValue ? all.FirstOrDefault(c => c.Id == downloadClientId.Value) : null;

            if (downloadClientId.HasValue && clientById == null)
            {
                throw new ResolveDownloadClientException("Download client with ID '{0}' could not be found", downloadClientId.Value);
            }

            if (downloadClientName.IsNotNullOrWhiteSpace() && clientByName == null)
            {
                throw new ResolveDownloadClientException("Download client with name '{0}' could not be found", downloadClientName);
            }

            if (clientByName == null && clientById == null)
            {
                return null;
            }

            if (clientByName != null && clientById != null && clientByName.Id != clientById.Id)
            {
                throw new ResolveDownloadClientException("Download client with name '{0}' does not match download client with ID '{1}'", downloadClientName, downloadClientId.Value);
            }

            var client = clientById ?? clientByName;

            if (!client.Enable)
            {
                throw new ResolveDownloadClientException("Download client '{0}' ({1}) is not enabled", client.Name, downloadClientId);
            }

            return clientById ?? clientByName;
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

        private IEnumerable<IDownloadClient> GetFilteredDownloadClients(DownloadProtocol downloadProtocol, int indexerId, bool filterBlockedClients, HashSet<int> tags, bool forSingleClient)
        {
            // Tags aren't required, but download clients with tags should not be picked unless there is at least one matching tag.
            // Defaulting to an empty HashSet ensures this is always checked.
            tags ??= new HashSet<int>();

            var blockedProviders = new HashSet<int>(_downloadClientStatusService.GetBlockedProviders().Select(v => v.ProviderId));
            var availableProviders = _downloadClientFactory.GetAvailableProviders().Where(v => v.Protocol == downloadProtocol).ToList();

            if (availableProviders.Empty())
            {
                return Enumerable.Empty<IDownloadClient>();
            }

            var matchingTagsClients = availableProviders.Where(i => i.Definition.Tags.Intersect(tags).Any()).ToList();

            availableProviders = matchingTagsClients.Count > 0 ?
                matchingTagsClients :
                availableProviders.Where(i => i.Definition.Tags.Empty()).ToList();

            if (availableProviders.Empty())
            {
                throw new DownloadClientUnavailableException("No download client was found without tags or a matching series tag. Please check your settings.");
            }

            if (indexerId > 0)
            {
                var indexer = _indexerFactory.Find(indexerId);

                if (indexer is { DownloadClientId: > 0 })
                {
                    var client = availableProviders.SingleOrDefault(d => d.Definition.Id == indexer.DownloadClientId);

                    if (client == null)
                    {
                        throw new DownloadClientUnavailableException($"Indexer specified download client does not exist for {indexer.Name}");
                    }

                    if (filterBlockedClients && blockedProviders.Contains(client.Definition.Id))
                    {
                        throw new DownloadClientUnavailableException($"Indexer specified download client is not available due to recent failures for {indexer.Name}");
                    }

                    return [client];
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
                    _logger.Trace("No non-blocked Download Client available{0}.", forSingleClient ? ", retrying blocked one" : $" for {downloadProtocol}, returning all clients");
                }
            }

            return availableProviders;
        }
    }
}
