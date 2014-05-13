using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;
using FluentValidation.Results;
using System.Linq;

namespace NzbDrone.Core.Indexers.Eztv
{
    public class Eztv : IndexerBase<EztvSettings>
    {
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

        public override IParseFeed Parser
        {
            get
            {
                return new BasicTorrentRssParser();
            }
        }

        public override IEnumerable<String> RecentFeed
        {
            get
            {
                yield return CreateRssUrl("feed/");
            }
        }

        public override IEnumerable<String> GetEpisodeSearchUrls(List<String> titles, Int32 tvRageId, Int32 seasonNumber, Int32 episodeNumber)
        {
            foreach (var seriesTitle in titles)
            {
                yield return CreateRssUrl(String.Format("search/index.php?show_name={0}&season={1}&episode={2}&mode=rss", seriesTitle, seasonNumber, episodeNumber));
            }
        }

        public override IEnumerable<String> GetDailyEpisodeSearchUrls(List<String> titles, Int32 tvRageId, DateTime date)
        {
            //EZTV doesn't support searching based on actual episode airdate. they only support release date.
            return Enumerable.Empty<String>();
        }

        public override IEnumerable<String> GetAnimeEpisodeSearchUrls(List<String> titles, Int32 tvRageId, Int32 absoluteEpisodeNumber)
        {
            return Enumerable.Empty<String>();
        }

        public override IEnumerable<String> GetSeasonSearchUrls(List<String> titles, Int32 tvRageId, Int32 seasonNumber, Int32 offset)
        {
            foreach (var seriesTitle in titles)
            {
                yield return CreateRssUrl(String.Format("search/index.php?show_name={0}&season={1}&mode=rss", seriesTitle, seasonNumber));
            }
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