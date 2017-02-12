using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Clients.DownloadStation.Responses;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Proxies
{
    public interface IDSMInfoProxy
    {
        string GetSerialNumber(DownloadStationSettings settings);
    }

    public class DSMInfoProxy : DiskStationProxyBase, IDSMInfoProxy
    {
        public DSMInfoProxy(IHttpClient httpClient, Logger logger) :
            base(httpClient, logger)
        {
        }

        public string GetSerialNumber(DownloadStationSettings settings)
        {
            var arguments = new Dictionary<string, object>() {
                { "api", "SYNO.DSM.Info" },
                { "version", "2" },
                { "method", "getinfo" }
            };

            var response = ProcessRequest<DSMInfoResponse>(DiskStationApi.DSMInfo, arguments, settings, "get serial number");
            return response.Data.SerialNumber;
        }
    }
}
