using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    public class TorrentRssParserSettingsCache : ITorrentRssParserSettingsCache
    {
        private readonly static Dictionary<string, TorrentRssIndexerParserSettings> Cache = new Dictionary<string, TorrentRssIndexerParserSettings>();
        private readonly static object CacheLocker = new object();

        /// <summary>
        /// Add or Update <paramref name="settings"/> for <paramref name="url"/>
        /// </summary>
        /// <param name="url">Url to add/update settings for</param>
        /// <param name="settings">Settings to update</param>
        public void AddOrUpdate(string url, TorrentRssIndexerParserSettings settings)
        {
            var lowerUrl = url.ToLowerInvariant();

            lock (CacheLocker)
            {
                if (Cache.ContainsKey(lowerUrl))
                {
                    Cache[lowerUrl] = settings;
                }
                else
                {
                    Cache.Add(lowerUrl, settings);
                }
            }
        }

        /// <summary>
        /// Retrieve <see cref="TorrentRssIndexerParserSettings"/> for <paramref name="url"/>
        /// </summary>
        /// <param name="url">Url to retrieve settings for</param>
        /// <returns>Settings or <c>null</c></returns>
        public TorrentRssIndexerParserSettings Get(string url)
        {
            var lowerUrl = url.ToLowerInvariant();

            lock (CacheLocker)
            {
                if (Cache.ContainsKey(lowerUrl))
                {
                    return Cache[lowerUrl];
                }
            }

            return null;
        }

        /// <summary>
        /// Check if cache contains <see cref="TorrentRssIndexerParserSettings"/> for <paramref name="url"/>
        /// </summary>
        /// <param name="url">Url to lookup</param>
        /// <returns><c>true</c> if settings found, otherwise <c>false</c></returns>
        public bool Contains(string url)
        {
            var lowerUrl = url.ToLowerInvariant();

            lock (CacheLocker)
            {
                if (Cache.ContainsKey(lowerUrl))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieve the count of currently cached Url's
        /// </summary>
        /// <returns>Count of Url's in cache</returns>
        public int Count()
        {
            lock (CacheLocker)
            {
                return Cache.Count;
            }
        }
    }
}
