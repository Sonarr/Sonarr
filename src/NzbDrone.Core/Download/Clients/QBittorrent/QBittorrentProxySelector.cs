using System;
using System.Collections.Generic;

using NLog;
using NzbDrone.Common.Cache;

using NzbDrone.Common.Http;


namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    public interface IQBittorrentProxy
    {
        bool IsApiSupported(QBittorrentSettings settings);
        Version GetApiVersion(QBittorrentSettings settings);
        string GetVersion(QBittorrentSettings settings);
        QBittorrentPreferences GetConfig(QBittorrentSettings settings);
        List<QBittorrentTorrent> GetTorrents(QBittorrentSettings settings);
        QBittorrentTorrentProperties GetTorrentProperties(string hash, QBittorrentSettings settings);
        List<QBittorrentTorrentFile> GetTorrentFiles(string hash, QBittorrentSettings settings);

        void AddTorrentFromUrl(string torrentUrl, QBittorrentSettings settings);
        void AddTorrentFromFile(string fileName, Byte[] fileContent, QBittorrentSettings settings);

        void RemoveTorrent(string hash, Boolean removeData, QBittorrentSettings settings);
        void SetTorrentLabel(string hash, string label, QBittorrentSettings settings);
        void AddLabel(string label, QBittorrentSettings settings);
        Dictionary<string, QBittorrentLabel> GetLabels(QBittorrentSettings settings);
        void SetTorrentSeedingConfiguration(string hash, TorrentSeedConfiguration seedConfiguration, QBittorrentSettings settings);
        void MoveTorrentToTopInQueue(string hash, QBittorrentSettings settings);
        void PauseTorrent(string hash, QBittorrentSettings settings);
        void ResumeTorrent(string hash, QBittorrentSettings settings);
        void SetForceStart(string hash, bool enabled, QBittorrentSettings settings);
    }

    public interface IQBittorrentProxySelector
    {
        IQBittorrentProxy GetProxy(QBittorrentSettings settings, bool force = false);
    }

    public class QBittorrentProxySelector : IQBittorrentProxySelector
    {
        private readonly IHttpClient _httpClient;
        private readonly ICached<IQBittorrentProxy> _proxyCache;
        private readonly Logger _logger;

        private readonly IQBittorrentProxy _proxyV1;
        private readonly IQBittorrentProxy _proxyV2;

        public  QBittorrentProxySelector(QBittorrentProxyV1 proxyV1,
                                         QBittorrentProxyV2 proxyV2,
                                         IHttpClient httpClient, 
                                         ICacheManager cacheManager,
                                         Logger logger)
        {
            _httpClient = httpClient;
            _proxyCache = cacheManager.GetCache<IQBittorrentProxy>(GetType());
            _logger = logger;

            _proxyV1 = proxyV1;
            _proxyV2 = proxyV2;
        }

        public IQBittorrentProxy GetProxy(QBittorrentSettings settings, bool force)
        {
            var proxyKey = $"{settings.Host}_{settings.Port}";

            if (force)
            {
                _proxyCache.Remove(proxyKey);
            }

            return _proxyCache.Get(proxyKey, () => FetchProxy(settings), TimeSpan.FromMinutes(10.0));      
        }

        private IQBittorrentProxy FetchProxy(QBittorrentSettings settings)
        {
            if (_proxyV2.IsApiSupported(settings))
            {
                _logger.Trace("Using qbitTorrent API v2");
                return _proxyV2;
            }

            if (_proxyV1.IsApiSupported(settings))
            {
                _logger.Trace("Using qbitTorrent API v1");
                return _proxyV1;
            }

            throw new DownloadClientException("Unable to determine qBittorrent API version");
        }
    }
}
