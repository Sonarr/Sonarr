using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.Wombles
{
    public class Wombles : BaseIndexer
    {
        public override IEnumerable<string> RecentFeed
        {
            get
            {
                return new[]
                           {
                               string.Format("http://nzb.isasecret.com/rss")
                           };
            }
        }

        public override string Name
        {
            get { return "WomblesIndex"; }
        }



        public override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            return new List<string>();
        }

        public override IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            return new List<string>();
        }


        public bool EnabledByDefault
        {
            get { return true; }
        }
    }
}