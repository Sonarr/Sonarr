using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public interface IDownloadClientFactory : IProviderFactory<IDownloadClient, DownloadClientDefinition>
    {
        List<IDownloadClient> DownloadHandlingEnabled(bool filterBlockedClients = true);
    }

    public class DownloadClientFactory : ProviderFactory<IDownloadClient, DownloadClientDefinition>, IDownloadClientFactory
    {
        private readonly IDownloadClientStatusService _downloadClientStatusService;
        private readonly Logger _logger;

        public DownloadClientFactory(IDownloadClientStatusService downloadClientStatusService,
                                     IDownloadClientRepository providerRepository,
                                     IEnumerable<IDownloadClient> providers,
                                     IServiceProvider container,
                                     IEventAggregator eventAggregator,
                                     Logger logger)
            : base(providerRepository, providers, container, eventAggregator, logger)
        {
            _downloadClientStatusService = downloadClientStatusService;
            _logger = logger;
        }

        protected override List<DownloadClientDefinition> Active()
        {
            return base.Active().Where(c => c.Enable).ToList();
        }

        public override void SetProviderCharacteristics(IDownloadClient provider, DownloadClientDefinition definition)
        {
            base.SetProviderCharacteristics(provider, definition);

            definition.Protocol = provider.Protocol;
        }

        public List<IDownloadClient> DownloadHandlingEnabled(bool filterBlockedClients = true)
        {
            var enabledClients = GetAvailableProviders();

            if (filterBlockedClients)
            {
                return FilterBlockedClients(enabledClients).ToList();
            }

            return enabledClients.ToList();
        }

        private IEnumerable<IDownloadClient> FilterBlockedClients(IEnumerable<IDownloadClient> clients)
        {
            var blockedIndexers = _downloadClientStatusService.GetBlockedProviders().ToDictionary(v => v.ProviderId, v => v);

            foreach (var client in clients)
            {
                DownloadClientStatus downloadClientStatus;
                if (blockedIndexers.TryGetValue(client.Definition.Id, out downloadClientStatus))
                {
                    _logger.Debug("Temporarily ignoring download client {0} till {1} due to recent failures.", client.Definition.Name, downloadClientStatus.DisabledTill.Value.ToLocalTime());
                    continue;
                }

                yield return client;
            }
        }

        public override ValidationResult Test(DownloadClientDefinition definition)
        {
            var result = base.Test(definition);

            if ((result == null || result.IsValid) && definition.Id != 0)
            {
                _downloadClientStatusService.RecordSuccess(definition.Id);
            }

            return result;
        }
    }
}
