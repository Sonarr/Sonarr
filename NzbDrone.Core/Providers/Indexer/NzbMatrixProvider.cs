using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NzbMatrixProvider : IndexerProviderBase
    {
        public NzbMatrixProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider,
            EpisodeProvider episodeProvider, ConfigProvider configProvider,
            HttpProvider httpProvider, IndexerProvider indexerProvider,
            HistoryProvider historyProvider, SabProvider sabProvider, IEnumerable<ExternalNotificationProviderBase> externalNotificationProvider)
            : base(seriesProvider, seasonProvider, episodeProvider,
            configProvider, httpProvider, indexerProvider, historyProvider,
            sabProvider, externalNotificationProvider)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                           {
                               string.Format(
                                   "http://rss.nzbmatrix.com/rss.php?page=download&username={0}&apikey={1}&subcat=6,41&english=1&scenename=1&num=50",
                                   _configProvider.NzbMatrixUsername,
                                   _configProvider.NzbMatrixApiKey)
                           };
            }
        }

        public override string Name
        {
            get { return "NzbMatrix"; }
        }

        public override bool SupportsBacklog
        {
            get { return true; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override IndexerType GetIndexerType()
        {
            return IndexerType.NzbMatrix;
        }
    }
}