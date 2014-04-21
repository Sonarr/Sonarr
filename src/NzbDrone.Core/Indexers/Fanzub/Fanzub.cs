using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Fanzub
{
    public class Fanzub : IndexerBase<NullConfig>
    {
        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Usenet;
            }
        }

        public override bool SupportsPaging
        {
            get
            {
                return false;
            }
        }

        public override bool SupportsSearching
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
                return new FanzubParser();
            }
        }

        public override IEnumerable<string> RecentFeed
        {
            get
            {
                yield return "http://fanzub.com/rss/?cat=anime";
            }
        }

        public override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int episodeNumber)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int offset)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, int tvRageId, DateTime date)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetAnimeEpisodeSearchUrls(string seriesTitle, int tvRageId, int absoluteEpisodeNumber)
        {
            return RecentFeed.Select(url => String.Format("{0}&q={1}%20{2}", url, seriesTitle, absoluteEpisodeNumber));
        }

        public override IEnumerable<string> GetSearchUrls(string query, int offset)
        {
            return new List<string>();
        }
    }
}
