using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Clients.DownloadStation.Responses;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Proxies
{
    public interface IDSMInfoProxy : IDiskStationProxy
    {
        DSMInfoResponse GetInfo(DownloadStationSettings settings);
    }

    public class DSMInfoProxy : DiskStationProxyBase, IDSMInfoProxy
    {
        public DSMInfoProxy(IHttpClient httpClient, ICacheManager cacheManager, Logger logger) :
            base(DiskStationApi.DSMInfo, "SYNO.DSM.Info", httpClient, cacheManager, logger)
        {
        }

        public DSMInfoResponse GetInfo(DownloadStationSettings settings)
        {
            var info = GetApiInfo(settings);

            var requestBuilder = BuildRequest(settings, "getinfo", info.MinVersion);

            var response = ProcessRequest<DSMInfoResponse>(requestBuilder, "get info", settings);

            return response.Data;
        }
    }
}
