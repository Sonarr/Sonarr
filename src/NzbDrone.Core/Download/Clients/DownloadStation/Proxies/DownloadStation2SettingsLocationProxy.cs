using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Clients.DownloadStation.Responses;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Proxies
{
    public interface IDownloadStation2SettingsLocationProxy
    {
        string GetDefaultDestination(DownloadStationSettings settings);
    }

    public class DownloadStation2SettingsLocationProxy : DiskStationProxyBase, IDownloadStation2SettingsLocationProxy
    {
        public DownloadStation2SettingsLocationProxy(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
            : base(DiskStationApi.DownloadStation2SettingsLocation, "SYNO.DownloadStation2.Settings.Location", httpClient, cacheManager, logger)
        {
        }

        public string GetDefaultDestination(DownloadStationSettings settings)
        {
            var info = GetApiInfo(settings);

            var requestBuilder = BuildRequest(settings, "get", info.MinVersion);

            var response = ProcessRequest<DownloadStation2SettingsLocationResponse>(requestBuilder, "get default destination folder", settings);

            return response.Data.Default_Destination;
        }
    }
}
