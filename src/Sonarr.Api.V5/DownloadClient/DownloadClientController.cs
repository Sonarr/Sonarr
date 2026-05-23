using FluentValidation;
using NzbDrone.Core.Download;
using NzbDrone.SignalR;
using Sonarr.Api.V5.Provider;
using Sonarr.Http;

namespace Sonarr.Api.V5.DownloadClient;

[V5ApiController]
public class DownloadClientController : ProviderControllerBase<DownloadClientResource, DownloadClientBulkResource, IDownloadClient, DownloadClientDefinition>
{
    public static readonly DownloadClientResourceMapper ResourceMapper = new();
    public static readonly DownloadClientBulkResourceMapper BulkResourceMapper = new();

    public DownloadClientController(IBroadcastSignalRMessage signalRBroadcaster, IDownloadClientFactory downloadClientFactory)
        : base(signalRBroadcaster, downloadClientFactory, "downloadclient", ResourceMapper, BulkResourceMapper)
    {
        SharedValidator.RuleFor(c => c.Priority).InclusiveBetween(1, 50);
    }
}
