using System.ServiceModel.Syndication;
using NzbDrone.Core.Providers.Core;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    internal class NzbMatrixFeedProvider : IndexerProviderBase
    {
        public NzbMatrixFeedProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider, EpisodeProvider episodeProvider, ConfigProvider configProvider, HttpProvider httpProvider, IRepository repository, IndexerProvider indexerProvider)
            : base(seriesProvider, seasonProvider, episodeProvider, configProvider, httpProvider, repository, indexerProvider)
        {
        }

        protected override string[] Url
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