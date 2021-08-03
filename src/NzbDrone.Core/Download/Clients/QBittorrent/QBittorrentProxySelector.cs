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
        bool IsTorrentLoaded(string hash, QBittorrentSettings settings);
        QBittorrentTorrentProperties GetTorrentProperties(string hash, QBittorrentSettings settings);
        List<QBittorrentTorrentFile> GetTorrentFiles(string hash, QBittorrentSettings settings);

        void AddTorrentFromUrl(string torrentUrl, TorrentSeedConfiguration seedConfiguration, QBittorrentSettings settings);
        void AddTorrentFromFile(string fileName, byte[] fileContent, TorrentSeedConfiguration seedConfiguration, QBittorrentSettings settings);

        void RemoveTorrent(string hash, bool removeData, QBittorrentSettings settings);
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
        Version GetApiVersion(QBittorrentSettings settings, bool force = false);
    }

    public class QBittorrentProxySelector : IQBittorrentProxySelector
    {
        private readonly ICached<Tuple<IQBittorrentProxy, Version>> _proxyCache;
        private readonly Logger _logger;

        private readonly IQBittorrentProxy _proxyV1;
        private readonly IQBittorrentProxy _proxyV2;

        public  QBittorrentProxySelector(QBittorrentProxyV1 proxyV1,
                                         QBittorrentProxyV2 proxyV2,
                                         ICacheManager cacheManager,
                                         Logger logger)
        {
            _proxyCache = cacheManager.GetCache<Tuple<IQBittorrentProxy, Version>>(GetType());
            _logger = logger;

            _proxyV1 = proxyV1;
            _proxyV2 = proxyV2;
        }

        public IQBittorrentProxy GetProxy(QBittorrentSettings settings, bool force)
        {
            return GetProxyCache(settings, force).Item1;
        }

        public Version GetApiVersion(QBittorrentSettings settings, bool force)
        {
            return GetProxyCache(settings, force).Item2;
        }

        private Tuple<IQBittorrentProxy, Version> GetProxyCache(QBittorrentSettings settings, bool force)
        {
            var proxyKey = $"{settings.Host}_{settings.Port}";

            if (force)
            {
                _proxyCache.Remove(proxyKey);
            }

            return _proxyCache.Get(proxyKey, () => FetchProxy(settings), TimeSpan.FromMinutes(10.0));
        }

        private Tuple<IQBittorrentProxy, Version> FetchProxy(QBittorrentSettings settings)
        {
            if (_proxyV2.IsApiSupported(settings))
            {
                _logger.Trace("Using qbitTorrent API v2");
                return Tuple.Create(_proxyV2, _proxyV2.GetApiVersion(settings));
            }

            if (_proxyV1.IsApiSupported(settings))
            {
                _logger.Trace("Using qbitTorrent API v1");
                return Tuple.Create(_proxyV1, _proxyV1.GetApiVersion(settings));
            }

            throw new DownloadClientException("Unable to determine qBittorrent API version");
        }
    }
}
