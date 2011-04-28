using System.Net;
using System.ServiceModel.Syndication;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NzbsRUsProvider : IndexerProviderBase
    {
        public NzbsRUsProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider, EpisodeProvider episodeProvider, ConfigProvider configProvider, HttpProvider httpProvider, IndexerProvider indexerProvider, HistoryProvider historyProvider, SabProvider sabProvider)
            : base(seriesProvider, seasonProvider, episodeProvider, configProvider, httpProvider, indexerProvider, historyProvider, sabProvider)
        {
        }

        protected override string[] Urls
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
            return item.Links[0].Uri.ToString();
        }

        protected override IndexerType GetIndexerType()
        {
            return IndexerType.NzbsRus;
        }
    }
}