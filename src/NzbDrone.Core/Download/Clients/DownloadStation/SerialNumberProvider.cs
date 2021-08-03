using System;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.Clients.DownloadStation.Proxies;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public interface ISerialNumberProvider
    {
        string GetSerialNumber(DownloadStationSettings settings);
    }

    public class SerialNumberProvider : ISerialNumberProvider
    {
        private readonly IDSMInfoProxy _proxy;
        private readonly ILogger _logger;

        private ICached<string> _cache;

        public SerialNumberProvider(ICacheManager cacheManager,
                                    IDSMInfoProxy proxy,
                                    Logger logger)
        {
            _proxy = proxy;
            _cache = cacheManager.GetCache<string>(GetType());
            _logger = logger;
        }

        public string GetSerialNumber(DownloadStationSettings settings)
        {
            try
            {
                return _cache.Get(settings.Host, () => GetHashedSerialNumber(settings), TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Could not get the serial number from Download Station {0}:{1}", settings.Host, settings.Port);
                throw;
            }
        }

        private string GetHashedSerialNumber(DownloadStationSettings settings)
        {
            var serialNumber = _proxy.GetSerialNumber(settings);
            return HashConverter.GetHash(serialNumber).ToHexString();
        }
    }
}
