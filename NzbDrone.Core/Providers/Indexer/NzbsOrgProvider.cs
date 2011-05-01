using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NzbsOrgProvider : IndexerProviderBase
    {
        public NzbsOrgProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider,
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
                                       string.Format("http://nzbs.org/rss.php?type=1&i={0}&h={1}&num=50&dl=1", _configProvider.NzbsOrgUId, _configProvider.NzbsOrgHash)
                                   };
            }
        }

        public override string Name
        {
            get { return "Nzbs.org"; }
        }

        public override bool SupportsBacklog
        {
            get { return false; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Id;
        }

        protected override IndexerType GetIndexerType()
        {
            return IndexerType.NzbsOrg;
        }
    }
}