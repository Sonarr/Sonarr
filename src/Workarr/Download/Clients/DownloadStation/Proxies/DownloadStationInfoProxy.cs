using NLog;
using Workarr.Cache;
using Workarr.Http;

namespace Workarr.Download.Clients.DownloadStation.Proxies
{
    public interface IDownloadStationInfoProxy : IDiskStationProxy
    {
        Dictionary<string, object> GetConfig(DownloadStationSettings settings);
    }

    public class DownloadStationInfoProxy : DiskStationProxyBase, IDownloadStationInfoProxy
    {
        public DownloadStationInfoProxy(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
            : base(DiskStationApi.DownloadStationInfo, "SYNO.DownloadStation.Info", httpClient, cacheManager, logger)
        {
        }

        public Dictionary<string, object> GetConfig(DownloadStationSettings settings)
        {
            var requestBuilder = BuildRequest(settings, "getConfig", 1);

            var response = ProcessRequest<Dictionary<string, object>>(requestBuilder, "get config", settings);

            return response.Data;
        }
    }
}
