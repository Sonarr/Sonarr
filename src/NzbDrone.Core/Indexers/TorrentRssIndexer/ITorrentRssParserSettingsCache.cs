namespace NzbDrone.Core.Indexers.TorrentRssIndexer
{
    public interface ITorrentRssParserSettingsCache
    {
        void AddOrUpdate(string url, TorrentRssIndexerParserSettings settings);

        TorrentRssIndexerParserSettings Get(string url);

        bool Contains(string url);

        int Count();
    }
}