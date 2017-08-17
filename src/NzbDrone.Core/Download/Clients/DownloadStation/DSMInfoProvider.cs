using System;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.Clients.DownloadStation.Proxies;
using NzbDrone.Core.Download.Clients.DownloadStation.Responses;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public interface IDSMInfoProvider
    {
        string GetSerialNumber(DownloadStationSettings settings);
        Version GetDSMVersion(DownloadStationSettings settings);
    }

    public class DSMInfoProvider : IDSMInfoProvider
    {
        private readonly IDSMInfoProxy _proxy;
        private ICached<DSMInfoResponse> _cache;
        private readonly ILogger _logger;

        public DSMInfoProvider(ICacheManager cacheManager,
                                    IDSMInfoProxy proxy,
                                    Logger logger)
        {
            _proxy = proxy;
            _cache = cacheManager.GetCache<DSMInfoResponse>(GetType());
            _logger = logger;
        }

        private DSMInfoResponse GetInfo(DownloadStationSettings settings)
        {
            return _cache.Get(settings.Host, () => _proxy.GetInfo(settings), TimeSpan.FromMinutes(5));
        }

        public string GetSerialNumber(DownloadStationSettings settings)
        {
            try
            {
                return HashConverter.GetHash(GetInfo(settings).SerialNumber).ToHexString();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Could not get the serial number from Download Station {0}:{1}", settings.Host, settings.Port);
                throw;
            }
        }

        public Version GetDSMVersion(DownloadStationSettings settings)
        {
            var info = GetInfo(settings);

            Regex regex = new Regex(@"DSM (?<version>[\d.]*)");

            var dsmVersion = regex.Match(info.Version).Groups["version"].Value;

            return new Version(dsmVersion);
        }
    }
}
