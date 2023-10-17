using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Localization;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderDeletedEvent<IDownloadClient>))]
    [CheckOn(typeof(ModelEvent<RootFolder>))]
    [CheckOn(typeof(ModelEvent<RemotePathMapping>))]

    public class DownloadClientSortingCheck : HealthCheckBase, IProvideHealthCheck
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly Logger _logger;

        public DownloadClientSortingCheck(IProvideDownloadClient downloadClientProvider,
                                          Logger logger,
                                          ILocalizationService localizationService)
            : base(localizationService)
        {
            _downloadClientProvider = downloadClientProvider;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            var clients = _downloadClientProvider.GetDownloadClients(true);

            foreach (var client in clients)
            {
                try
                {
                    var clientName = client.Definition.Name;
                    var status = client.GetStatus();

                    if (status.SortingMode.IsNotNullOrWhiteSpace())
                    {
                        return new HealthCheck(GetType(),
                            HealthCheckResult.Warning,
                            _localizationService.GetLocalizedString("DownloadClientSortingHealthCheckMessage", new Dictionary<string, object>
                            {
                                { "downloadClientName", clientName },
                                { "sortingMode", status.SortingMode }
                            }),
                            "#download-folder-and-library-folder-not-different-folders");
                    }
                }
                catch (DownloadClientException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unknown error occurred in DownloadClientSortingCheck HealthCheck");
                }
            }

            return new HealthCheck(GetType());
        }
    }
}
