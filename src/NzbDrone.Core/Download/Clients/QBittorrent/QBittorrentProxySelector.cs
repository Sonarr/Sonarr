using System;
using System.Collections.Generic;

using NLog;
using NzbDrone.Common.Cache;

using NzbDrone.Common.Http;


namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    // API https://github.com/qbittorrent/qBittorrent/wiki/WebUI-API-Documentation

    public interface IQBittorrentProxy
    {
        System.Version GetVersion(QBittorrentSettings settings);
        QBittorrentPreferences GetConfig(QBittorrentSettings settings);
        List<QBittorrentTorrent> GetTorrents(QBittorrentSettings settings);

        void AddTorrentFromUrl(string torrentUrl, QBittorrentSettings settings);
        void AddTorrentFromFile(string fileName, Byte[] fileContent, QBittorrentSettings settings);

        void RemoveTorrent(string hash, Boolean removeData, QBittorrentSettings settings);
        void SetTorrentLabel(string hash, string label, QBittorrentSettings settings);
        void MoveTorrentToTopInQueue(string hash, QBittorrentSettings settings);
        void PauseTorrent(string hash, QBittorrentSettings settings);
        void ResumeTorrent(string hash, QBittorrentSettings settings);
        void SetForceStart(string hash, bool enabled, QBittorrentSettings settings);
    }

    public interface IQBittorrentProxySelector
    {
        IQBittorrentProxy GetProxy(QBittorrentSettings settings);
    }

    public class QBittorrentProxySelector : IQBittorrentProxySelector
    {
        private readonly IHttpClient _httpClient;
        private readonly ICacheManager _cacheManager;
        private readonly Logger _logger;

        public  QBittorrentProxySelector(IHttpClient httpClient, ICacheManager cacheManager, Logger logger)
        {
            _httpClient = httpClient;
            _cacheManager = cacheManager;
            _logger = logger;
        }

        public IQBittorrentProxy GetProxy(QBittorrentSettings settings)
        {
            //Try to get API version using V2 API call... If it fails, we will fall back to V1 API
            var qBitV2 = new QBittorrentProxyV2(_httpClient, _cacheManager, _logger);
            var version = qBitV2.GetVersion(settings);

            if (version.ToString() == "")
            {
                // Return V1 Proxy
                _logger.Debug("Using V1 Proxy");
                return new QBittorrentProxyV1(_httpClient, _cacheManager, _logger);
            }
            else
            {
                // Return V2 Proxy
                _logger.Debug("Using V2 Proxy");
                return qBitV2;
            }

        }
    }
}
