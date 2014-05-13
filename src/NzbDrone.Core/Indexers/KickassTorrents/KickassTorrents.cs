using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;
using FluentValidation.Results;

namespace NzbDrone.Core.Indexers.KickassTorrents
{
    public class KickassTorrents : IndexerBase<KickassTorrentsSettings>
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
                return 25;
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
                return new KickassTorrentsRssParser();
            }
        }

        public override IEnumerable<String> RecentFeed
        {
            get
            {
                yield return CreateRssUrl("tv/?rss=1&field=time_add&sorder=desc");
            }
        }

        public override IEnumerable<String> GetEpisodeSearchUrls(List<String> titles, Int32 tvRageId, Int32 seasonNumber, Int32 episodeNumber)
        {
            foreach (var seriesTitle in titles)
            {
                yield return CreateSearchUrl(String.Format("{0} category:tv season:{1} episode:{2}", seriesTitle.Replace('+', ' '), seasonNumber, episodeNumber), 0);
                yield return CreateSearchUrl(String.Format("{0} S{1:00}E{2:00} category:tv", seriesTitle.Replace('+', ' '), seasonNumber, episodeNumber), 0);
            }
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
            foreach (var seriesTitle in titles)
            {
                yield return CreateSearchUrl(String.Format("{0} category:tv season:{1}", seriesTitle.Replace('+', ' '), seasonNumber), offset);
            }
        }

        public override IEnumerable<String> GetSearchUrls(String query, Int32 offset)
        {
            yield return CreateSearchUrl(query, offset);
        }

        private String CreateSearchUrl(String query, Int32 offset)
        {
            if (Settings.VerifiedOnly)
            {
                query += " verified:1";
            }

            if (offset == 0)
            {
                return CreateRssUrl(String.Format("usearch/{0}/?rss=1&field=seeders&sorder=desc", query));
            }
            else
            {
                return CreateRssUrl(String.Format("usearch/{0}/{1}/?rss=1&field=seeders&sorder=desc", query, 1 + offset / SupportedPageSize));
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