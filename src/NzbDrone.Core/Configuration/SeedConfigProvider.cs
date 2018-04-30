using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Configuration
{
    public class SeedConfigProvider: ISeedConfigProvider
    {
        private readonly IIndexerFactory _indexerFactory;

        public SeedConfigProvider(IIndexerFactory indexerFactory)
        {
            _indexerFactory = indexerFactory;
        }

        public TorrentSeedConfiguration GetSeedConfiguration(ReleaseInfo release)
        {
            var seedConfig = new TorrentSeedConfiguration();

            if (release.DownloadProtocol != DownloadProtocol.Torrent) return seedConfig;

            var indexer = _indexerFactory.Get(release.IndexerId);

            if (indexer.Settings is ITorrentIndexerSettings torrentIndexerSettings)
            {
                seedConfig.Ratio = torrentIndexerSettings.SeedRatio;
            }

            return seedConfig;
        }
    }
}
