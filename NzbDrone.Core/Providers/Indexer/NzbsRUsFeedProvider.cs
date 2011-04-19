using System.ServiceModel.Syndication;
using NzbDrone.Core.Providers.Core;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    internal class NzbsRUsFeedProvider : IndexerProviderBase
    {
        public NzbsRUsFeedProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider, EpisodeProvider episodeProvider, ConfigProvider configProvider, HttpProvider httpProvider, IRepository repository, IndexerProvider indexerProvider)
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
                                   "http://www.nzbsrus.com/rssfeed.php?cat=91,75&i={0}&h={1}",
                                   _configProvider.NzbsrusUId,
                                   _configProvider.NzbsrusHash)
                           };
            }
        }


        public override string Name
        {
            get { return "NzbsRUs"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].ToString();
        }
    }
}