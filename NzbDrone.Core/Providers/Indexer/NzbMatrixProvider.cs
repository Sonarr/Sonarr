using System.ServiceModel.Syndication;
using NzbDrone.Core.Providers.Core;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NzbMatrixProvider : IndexerProviderBase
    {
        public NzbMatrixProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider, EpisodeProvider episodeProvider, ConfigProvider configProvider, HttpProvider httpProvider, IndexerProvider indexerProvider, HistoryProvider historyProvider) : base(seriesProvider, seasonProvider, episodeProvider, configProvider, httpProvider, indexerProvider, historyProvider)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                           {
                               string.Format(
                                   "http://rss.nzbmatrix.com/rss.php?page=download&username={0}&apikey={1}&subcat=6,41&english=1&scenename=1",
                                   _configProvider.NzbMatrixUsername,
                                   _configProvider.NzbMatrixApiKey)
                           };
            }
        }
        

        public override string Name
        {
            get { return "NzbMatrix"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].ToString();
        }
    }
}