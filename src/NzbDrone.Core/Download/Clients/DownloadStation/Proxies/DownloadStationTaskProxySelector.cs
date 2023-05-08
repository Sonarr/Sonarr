using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Cache;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Proxies
{
    public interface IDownloadStationTaskProxy : IDiskStationProxy
    {
        bool IsApiSupported(DownloadStationSettings settings);
        IEnumerable<DownloadStationTask> GetTasks(DownloadStationSettings settings);
        void RemoveTask(string downloadId, DownloadStationSettings settings);
        void AddTaskFromUrl(string url, string downloadDirectory, DownloadStationSettings settings);
        void AddTaskFromData(byte[] data, string filename, string downloadDirectory, DownloadStationSettings settings);
    }

    public interface IDownloadStationTaskProxySelector
    {
        IDownloadStationTaskProxy GetProxy(DownloadStationSettings settings);
    }

    public class DownloadStationTaskProxySelector : IDownloadStationTaskProxySelector
    {
        private readonly ICached<IDownloadStationTaskProxy> _proxyCache;
        private readonly Logger _logger;

        private readonly IDownloadStationTaskProxy _proxyV1;
        private readonly IDownloadStationTaskProxy _proxyV2;

        public DownloadStationTaskProxySelector(DownloadStationTaskProxyV1 proxyV1, DownloadStationTaskProxyV2 proxyV2, ICacheManager cacheManager, Logger logger)
        {
            _proxyCache = cacheManager.GetCache<IDownloadStationTaskProxy>(GetType(), "taskProxy");
            _logger = logger;

            _proxyV1 = proxyV1;
            _proxyV2 = proxyV2;
        }

        public IDownloadStationTaskProxy GetProxy(DownloadStationSettings settings)
        {
            return GetProxyCache(settings);
        }

        private IDownloadStationTaskProxy GetProxyCache(DownloadStationSettings settings)
        {
            var propKey = $"{settings.Host}_{settings.Port}";

            return _proxyCache.Get(propKey, () => FetchProxy(settings), TimeSpan.FromMinutes(10.0));
        }

        private IDownloadStationTaskProxy FetchProxy(DownloadStationSettings settings)
        {
            if (_proxyV2.IsApiSupported(settings))
            {
                _logger.Trace("Using DownloadStation Task API v2");
                return _proxyV2;
            }

            if (_proxyV1.IsApiSupported(settings))
            {
                _logger.Trace("Using DownloadStation Task API v1");
                return _proxyV1;
            }

            throw new DownloadClientException("Unable to determine DownloadStations Task API version");
        }
    }
}
