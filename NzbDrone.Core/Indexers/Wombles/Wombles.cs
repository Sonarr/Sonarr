using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.Wombles
{
    public class Wombles : IndexerBase
    {
        public override string Name
        {
            get { return "WomblesIndex"; }
        }

        public override IParseFeed Parser
        {
            get
            {
                return new WomblesParser();
            }
        }

        public override IEnumerable<string> RecentFeed
        {
            get { yield return "http://nzb.isasecret.com/rss/?sec=TV&fr=false"; }
        }

        public override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int rageTvId, int seasonNumber, int episodeNumber)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int rageTvId, int seasonNumber)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, int rageTvId, DateTime date)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int rageTvId, int seasonNumber, int episodeWildcard)
        {
            return new List<string>();
        }
    }
}