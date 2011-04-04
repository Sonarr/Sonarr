using System.ServiceModel.Syndication;
using System.Xml;
using NLog;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers
{
    abstract class FeedProviderBase
    {
        private readonly ISeriesProvider _seriesProvider;
        private readonly ISeasonProvider _seasonProvider;
        private readonly IEpisodeProvider _episodeProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected FeedProviderBase(ISeriesProvider seriesProvider, ISeasonProvider seasonProvider, IEpisodeProvider episodeProvider)
        {
            _seriesProvider = seriesProvider;
            _seasonProvider = seasonProvider;
            _episodeProvider = episodeProvider;
        }


        /// <summary>
        /// Gets the source URL for the feed
        /// </summary>
        protected abstract string[] URL { get; }

        /// <summary>
        /// Gets the name for this feed
        /// </summary>
        protected abstract string Name { get; }


        /// <summary>
        /// Generates direct link to download an NZB
        /// </summary>
        /// <param name="item">RSS Feed item to generate the link for</param>
        /// <returns>Download link URL</returns>
        protected abstract string NzbDownloadUrl(SyndicationItem item);


        /// <summary>
        /// Parses the RSS feed item and.
        /// </summary>
        /// <param name="item">RSS feed item to parse</param>
        /// <returns>Detailed episode info</returns>
        protected EpisodeParseResult ParseFeed(SyndicationItem item)
        {
            var episodeParseResult = Parser.ParseEpisodeInfo(item.Title.ToString());
            var seriesInfo = _seriesProvider.FindSeries(episodeParseResult.SeriesTitle);

            if (seriesInfo != null)
            {
                episodeParseResult.SeriesId = seriesInfo.SeriesId;
                episodeParseResult.SeriesTitle = seriesInfo.Title;
                return episodeParseResult;
            }

            Logger.Debug("Unable to map {0} to any of series in database", episodeParseResult.SeriesTitle);
            return null;

        }



        /// <summary>
        /// Fetches RSS feed and process each news item.
        /// </summary>
        public void Fetch()
        {
            Logger.Info("Fetching feeds from " + Name);

            foreach (var url in URL)
            {
                var feed = SyndicationFeed.Load(XmlReader.Create(url)).Items;

                foreach (var item in feed)
                {
                    ProcessItem(item);
                }
            }

            Logger.Info("Finished processing feeds from " + Name);
        }

        private void ProcessItem(SyndicationItem feedItem)
        {
            var parseResult = ParseFeed(feedItem);

            if (!_seriesProvider.IsMonitored(parseResult.SeriesId))
            {
                Logger.Debug("{0} is present in the DB but not tracked. skipping.", parseResult.SeriesTitle);
            }

            if (!_seriesProvider.QualityWanted(parseResult.SeriesId, parseResult.Quality))
            {
                Logger.Debug("Post doesn't meet the quality requirements [{0}]. skipping.", parseResult.Quality);
            }

            if (_seasonProvider.IsIgnored(parseResult.SeriesId, parseResult.SeasonNumber))
            {
                Logger.Debug("Season {0} is currently set to ignore. skipping.", parseResult.SeasonNumber);
            }

            if (!_episodeProvider.IsNeeded(parseResult))
            {
                Logger.Debug("Episode {0} is not needed. skipping.", parseResult);
            }


        }
    }

}
