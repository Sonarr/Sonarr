using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Indexers.Torrentleech
{
    public class Torrentleech : IndexerBase<TorrentleechSettings>
    {
        private const String BaseUrl = "http://rss.torrentleech.org";

        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Torrent;
            }
        }

        public override Int32 SupportedPageSize
        {
            get
            {
                return 0;
            }
        }

        public override Boolean SupportsSearch
        {
            get
            {
                return false;
            }
        }

        public override IParseFeed Parser
        {
            get
            {
                return new TorrentleechRssParser();
            }
        }

        public override IEnumerable<String> RecentFeed
        {
            get
            {
                yield return CreateRssUrl(Settings.ApiKey);
            }
        }

        public override IEnumerable<String> GetEpisodeSearchUrls(List<String> titles, Int32 tvRageId, Int32 seasonNumber, Int32 episodeNumber)
        {
            return Enumerable.Empty<String>();
        }

        public override IEnumerable<String> GetDailyEpisodeSearchUrls(List<String> titles, Int32 tvRageId, DateTime date)
        {
            return Enumerable.Empty<String>();
        }

        public override IEnumerable<String> GetAnimeEpisodeSearchUrls(List<String> titles, Int32 tvRageId, Int32 absoluteEpisodeNumber)
        {
            return Enumerable.Empty<String>();
        }

        public override IEnumerable<String> GetSeasonSearchUrls(List<String> titles, Int32 tvRageId, Int32 seasonNumber, Int32 offset)
        {
            return Enumerable.Empty<String>();
        }

        public override IEnumerable<String> GetSearchUrls(String query, Int32 offset)
        {
            return Enumerable.Empty<String>();
        }

        private String CreateRssUrl(String relativeUrl)
        {
            return String.Format("{0}/{1}", Settings.BaseUrl.Trim().TrimEnd('/'), relativeUrl);
        }

        public override ValidationResult Test()
        {
            return new ValidationResult();
        }
    }
}
