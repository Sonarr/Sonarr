using NzbDrone.Core.Download;
using Sonarr.Http;

namespace Sonarr.Api.V3.DownloadClient
{
    [V3ApiController]
    public class DownloadClientController : ProviderControllerBase<DownloadClientResource, IDownloadClient, DownloadClientDefinition>
    {
        public static readonly DownloadClientResourceMapper ResourceMapper = new DownloadClientResourceMapper();

        public DownloadClientController(IDownloadClientFactory downloadClientFactory)
            : base(downloadClientFactory, "downloadclient", ResourceMapper)
        {
        }
    }
}
