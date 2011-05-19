using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NewzbinProvider : IndexerProviderBase
    {
        public NewzbinProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider,
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
                                       "http://www.newzbin.com/browse/category/p/tv?feed=rss&hauth=1"
                                   };
            }
        }

        protected override NetworkCredential Credentials
        {
            get { return new NetworkCredential(_configProvider.NewzbinUsername, _configProvider.NewzbinPassword); }
        }

        public override string Name
        {
            get { return "Newzbin"; }
        }

        public override bool SupportsBacklog
        {
            get { return false; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Id + "/nzb";
        }

        protected override EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            var quality = Parser.ParseQuality(item.Summary.Text);
            var proper = Parser.ParseProper(item.Summary.Text);

            currentResult.Quality = quality;
            currentResult.Proper = proper;

            return currentResult;
        }

    }
}