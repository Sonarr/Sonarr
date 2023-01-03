using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
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
                                      Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            var clients = _downloadClientProvider.GetDownloadClients();

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
                            $"Download client {clientName} has {status.SortingMode} sorting enabled for Sonarr's category. You should disable sorting in your download client to avoid import issues.",
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
