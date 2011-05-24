using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Indexer
{
    public abstract class IndexerBase
    {
        protected readonly Logger _logger;
        private readonly HttpProvider _httpProvider;
        protected readonly ConfigProvider _configProvider;
        private readonly IndexerProvider _indexerProvider;

        protected IndexerBase(HttpProvider httpProvider, ConfigProvider configProvider, IndexerProvider indexerProvider)
        {
            _httpProvider = httpProvider;
            _configProvider = configProvider;
            _indexerProvider = indexerProvider;

            _logger = LogManager.GetLogger(GetType().ToString());
        }

        /// <summary>
        ///   Gets the name for the feed
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///   Gets the source URL for the feed
        /// </summary>
        protected abstract string[] Urls { get; }

        /// <summary>
        /// Gets the credential.
        /// </summary>
        protected virtual NetworkCredential Credentials
        {
            get { return null; }
        }

        public IndexerSetting Settings
        {
            get
            {
                return _indexerProvider.GetSettings(GetType());
            }
        }

        /// <summary>
        ///   Fetches RSS feed and process each news item.
        /// </summary>
        public List<EpisodeParseResult> Fetch()
        {
            _logger.Debug("Fetching feeds from " + Settings.Name);

            var result = new List<EpisodeParseResult>();

            foreach (var url in Urls)
            {
                try
                {
                    _logger.Trace("Downloading RSS " + url);

                    var reader = new SyndicationFeedXmlReader(_httpProvider.DownloadStream(url, Credentials));
                    var feed = SyndicationFeed.Load(reader).Items;

                    foreach (var item in feed)
                    {
                        try
                        {
                            var parsedEpisode = ParseFeed(item);
                            if (parsedEpisode != null)
                            {
                                parsedEpisode.NzbUrl = NzbDownloadUrl(item);
                                parsedEpisode.Indexer = Name;
                                parsedEpisode.NzbTitle = item.Title.Text;
                                result.Add(parsedEpisode);
                            }
                        }
                        catch (Exception itemEx)
                        {
                            _logger.ErrorException("An error occurred while processing feed item", itemEx);
                        }

                    }
                }
                catch (Exception feedEx)
                {
                    _logger.ErrorException("An error occurred while processing feed", feedEx);
                }
            }

            _logger.Info("Finished processing feeds from " + Settings.Name);
            return result;
        }

        /// <summary>
        ///   Parses the RSS feed item
        /// </summary>
        /// <param name = "item">RSS feed item to parse</param>
        /// <returns>Detailed episode info</returns>
        public EpisodeParseResult ParseFeed(SyndicationItem item)
        {
            var episodeParseResult = Parser.ParseEpisodeInfo(item.Title.Text);

            return CustomParser(item, episodeParseResult);
        }

        /// <summary>
        /// This method can be overwritten to provide indexer specific info parsing
        /// </summary>
        /// <param name="item">RSS item that needs to be parsed</param>
        /// <param name="currentResult">Result of the built in parse function.</param>
        /// <returns></returns>
        protected virtual EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            return currentResult;
        }

        /// <summary>
        ///   Generates direct link to download an NZB
        /// </summary>
        /// <param name = "item">RSS Feed item to generate the link for</param>
        /// <returns>Download link URL</returns>
        protected abstract string NzbDownloadUrl(SyndicationItem item);
    }
}