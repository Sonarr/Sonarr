using System;
using System.ServiceModel.Syndication;
using System.Xml;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    abstract class FeedProviderBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets the source URL for the feed
        /// </summary>
        protected abstract string[] URL { get; }

        /// <summary>
        /// Gets the name for this feed
        /// </summary>
        protected abstract string Name { get; }


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

        private void ProcessItem(SyndicationItem item)
        {
            var parseResult = ParseFeed(item);
        }


        public void DownloadIfWanted(NzbInfoModel nzb, Indexer indexer)
        {
            if (nzb.IsPassworded())
            {
                Logger.Debug("Skipping Passworded Report {0}", nzb.Title);
                return;
            }

            var episodeParseResults = Parser.ParseEpisodeInfo(nzb.Title);

            if (episodeParseResults.Episodes.Count > 0)
            {
                //ProcessStandardItem(nzb, indexer, episodeParseResults);
                return;
            }

            //Handles Full Season NZBs
            var seasonParseResult = Parser.ParseSeasonInfo(nzb.Title);

            if (seasonParseResult != null)
            {
                //ProcessFullSeasonItem(nzb, indexer, seasonParseResult);
                return;
            }

            Logger.Debug("Unsupported Title: {0}", nzb.Title);

        }


        protected EpisodeParseResult ParseFeed(SyndicationItem item)
        {
            return Parser.ParseEpisodeInfo(item.Title.ToString());
        }
    }

}
