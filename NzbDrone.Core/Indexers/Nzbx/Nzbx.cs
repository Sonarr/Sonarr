using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.Nzbx
{
    public class Nzbx : IndexerBase
    {
        public override IParseFeed Parser
        {
            get
            {
                return new NzbxParser();
            }
        }

        public override string Name
        {
            get { return "nzbx"; }
        }

        public override IEnumerable<string> RecentFeed
        {
            get
            {
                return new[] { String.Format("https://nzbx.co/api/recent?category=tv") };
            }
        }


        public override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            yield return String.Format("https://nzbx.co/api/search?q={0}+S{1:00}E{2:00}", seriesTitle, seasonNumber, episodeNumber);
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            yield return String.Format("https://nzbx.co/api/search?q={0}+{1:yyyy MM dd}", seriesTitle, date);
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            yield return String.Format("https://nzbx.co/api/search?q={0}+S{1:00}", seriesTitle, seasonNumber);
        }

        public override IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            yield return String.Format("https://nzbx.co/api/search?q={0}+S{1:00}E{2}", seriesTitle, seasonNumber, episodeWildcard);
        }
    }
}
