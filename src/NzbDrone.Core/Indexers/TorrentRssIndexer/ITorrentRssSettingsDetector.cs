namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    public interface ITorrentRssSettingsDetector
    {
        /// <summary>
        /// Detect settings for Parser, based on URL
        /// </summary>
        /// <param name="settings">Indexer Settings to use for Parser</param>
        /// <returns>Parsed Settings or <c>null</c></returns>
        TorrentRssIndexerParserSettings Detect(TorrentRssIndexerSettings settings);
    }
}