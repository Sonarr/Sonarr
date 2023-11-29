using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

    public class DownloadClientRootFolderCheck : HealthCheckBase, IProvideHealthCheck
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IRootFolderService _rootFolderService;
        private readonly Logger _logger;

        public DownloadClientRootFolderCheck(IProvideDownloadClient downloadClientProvider,
                                             IRootFolderService rootFolderService,
                                             Logger logger,
                                             ILocalizationService localizationService)
            : base(localizationService)
        {
            _downloadClientProvider = downloadClientProvider;
            _rootFolderService = rootFolderService;
            _logger = logger;
        }

        public override HealthCheck Check()
        {
            // Only check clients not in failure status, those get another message
            var clients = _downloadClientProvider.GetDownloadClients(true);

            var rootFolders = _rootFolderService.All();

            foreach (var client in clients)
            {
                try
                {
                    var status = client.GetStatus();
                    var folders = status.OutputRootFolders.Where(folder => rootFolders.Any(r => r.Path.PathEquals(folder.FullPath)));

                    foreach (var folder in folders)
                    {
                        return new HealthCheck(GetType(),
                            HealthCheckResult.Warning,
                            _localizationService.GetLocalizedString("DownloadClientRootFolderHealthCheckMessage", new Dictionary<string, object>
                            {
                                { "downloadClientName", client.Definition.Name },
                                { "path", folder.FullPath }
                            }),
                            "#downloads-in-root-folder");
                    }
                }
                catch (DownloadClientException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (HttpRequestException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unknown error occurred in DownloadClientRootFolderCheck HealthCheck");
                }
            }

            return new HealthCheck(GetType());
        }
    }
}
