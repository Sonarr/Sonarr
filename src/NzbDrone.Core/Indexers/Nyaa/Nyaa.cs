using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;
using FluentValidation.Results;

namespace NzbDrone.Core.Indexers.Nyaa
{
    public class Nyaa : IndexerBase<NyaaSettings>
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
                return 100;
            }
        }

        public override Boolean SupportsSearch
        {
            get
            {
                return true;
            }
        }

        public override IParseFeed Parser
        {
            get
            {
                return new NyaaRssParser();
            }
        }

        public override IEnumerable<String> RecentFeed
        {
            get
            {
                yield return CreateRssUrl("?page=rss&cats=1_37&filter=1");
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
            foreach (var seriesTitle in titles)
            {
                yield return CreateSearchUrl(String.Format("{0}+{1:0}", seriesTitle.Replace(' ', '+'), absoluteEpisodeNumber), 0);
                if (absoluteEpisodeNumber < 10)
                {
                    yield return CreateSearchUrl(String.Format("{0}+{1:00}", seriesTitle.Replace(' ', '+'), absoluteEpisodeNumber), 0);
                }
            }
        }

        public override IEnumerable<String> GetSeasonSearchUrls(List<String> titles, Int32 tvRageId, Int32 seasonNumber, Int32 offset)
        {
            return Enumerable.Empty<String>();
        }

        public override IEnumerable<String> GetSearchUrls(String query, Int32 offset)
        {
            yield return CreateSearchUrl(query, offset);
        }

        private String CreateSearchUrl(String query, Int32 offset)
        {
            if (offset == 0)
            {
                return CreateRssUrl(String.Format("?page=rss&cats=1_37&filter=1&term={0}", query));
            }
            else
            {
                return CreateRssUrl(String.Format("?page=rss&cats=1_37&filter=1&term={0}&offset={1}", query, 1 + offset / SupportedPageSize));
            }
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