using System.ServiceModel.Syndication;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public class NewzbinProvider : IndexerProviderBase
    {
        public NewzbinProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider, EpisodeProvider episodeProvider, ConfigProvider configProvider, HttpProvider httpProvider, IndexerProvider indexerProvider, HistoryProvider historyProvider) : base(seriesProvider, seasonProvider, episodeProvider, configProvider, httpProvider, indexerProvider, historyProvider)
        {
        }

        protected override string[] Urls
        {
            get
            {
                return new[]
                                   {
                                       string.Format("http://www.newzbin.com", _configProvider.NewzbinUsername, _configProvider.NewzbinPassword)
                                   };
            }
        }

        public override string Name
        {
            get { return "Newzbin"; }
        }


        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Id;
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