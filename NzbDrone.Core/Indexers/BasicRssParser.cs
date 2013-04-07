using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Syndication;
using NLog;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Indexers
{
    public interface IParseFeed
    {
        IEnumerable<IndexerParseResult> Process(Stream source);
    }

    public class BasicRssParser : IParseFeed
    {
        private readonly Logger _logger;

        public BasicRssParser()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public IEnumerable<IndexerParseResult> Process(Stream source)
        {
            var reader = new SyndicationFeedXmlReader(source);
            var feed = SyndicationFeed.Load(reader).Items;

            var result = new List<IndexerParseResult>();

            foreach (var syndicationItem in feed)
            {
                try
                {
                    var parsedEpisode = ParseFeed(syndicationItem);
                    if (parsedEpisode != null)
                    {
                        parsedEpisode.NzbUrl = GetNzbUrl(syndicationItem);
                        parsedEpisode.NzbInfoUrl = GetNzbUrl(syndicationItem);
                        result.Add(parsedEpisode);
                    }
                }
                catch (Exception itemEx)
                {
                    itemEx.Data.Add("Item", syndicationItem.Title);
                    _logger.ErrorException("An error occurred while processing feed item", itemEx);
                }
            }

            return result;
        }


        protected virtual string GetTitle(SyndicationItem syndicationItem)
        {
            return syndicationItem.Title.Text;
        }

        protected virtual string GetNzbUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected virtual string GetNzbInfoUrl(SyndicationItem item)
        {
            return string.Empty;
        }

        protected virtual IndexerParseResult PostProcessor(SyndicationItem item, IndexerParseResult currentResult)
        {
            return currentResult;
        }

        private IndexerParseResult ParseFeed(SyndicationItem item)
        {
            var title = GetTitle(item);

            var episodeParseResult = Parser.ParseTitle<IndexerParseResult>(title);
            if (episodeParseResult != null)
            {
                episodeParseResult.Age = DateTime.Now.Date.Subtract(item.PublishDate.Date).Days;
                episodeParseResult.OriginalString = title;
                episodeParseResult.SceneSource = true;
                episodeParseResult.ReleaseGroup = ParseReleaseGroup(title);
            }

            _logger.Trace("Parsed: {0} from: {1}", episodeParseResult, item.Title.Text);

            return PostProcessor(item, episodeParseResult);
        }

        private static string ParseReleaseGroup(string title)
        {
            title = title.Trim();
            var index = title.LastIndexOf('-');

            if (index < 0)
                index = title.LastIndexOf(' ');

            if (index < 0)
                return String.Empty;

            var group = title.Substring(index + 1);

            if (group.Length == title.Length)
                return String.Empty;

            return group;
        }
    }
}