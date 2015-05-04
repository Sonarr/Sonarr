namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    public interface ITorrentRssParserFactory
    {
        /// <summary>
        /// Get configured Parser based on <paramref name="settings"/>
        /// </summary>
        /// <param name="settings">Indexer Settings to use for Parser</param>
        /// <returns>Configured Parser</returns>
        TorrentRssParser GetParser(TorrentRssIndexerSettings settings);
    }
}
