namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    using System;

    public interface ITorrentRssSettingsDetector
    {
        /// <summary>
        /// Detect settings for Parser, based on URL
        /// </summary>
        /// <param name="settings">Indexer Settings to use for Parser</param>
        /// <param name="fetchIndexerResponseFunc">Func to retrieve Feed</param>
        /// <returns>Parsed Settings or <c>null</c></returns>
        TorrentRssIndexerParserSettings Detect(TorrentRssIndexerSettings settings, Func<IndexerRequest, IndexerResponse> fetchIndexerResponseFunc);
    }
}