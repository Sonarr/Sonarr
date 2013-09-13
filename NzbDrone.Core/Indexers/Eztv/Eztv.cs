using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.Eztv
{
    public class Eztv : IndexerBase
    {
        public override string Name
        {
            get { return "Eztv"; }
        }

        public override bool EnableByDefault
        {
            get { return false; }
        }

        public override IParseFeed Parser
        {
            get
            {
                return new BasicTorrentRssParser();
            }
        }

        public override IEnumerable<string> RecentFeed
        {
            get
            {
                return new[]
                           {
                              "http://www.ezrss.it/feed/"
                           };
            }
        }

        public override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int episodeNumber)
        {
            yield return string.Format("http://www.ezrss.it/search/index.php?show_name={0}&season={1}&episode={2}&mode=rss", seriesTitle, seasonNumber, episodeNumber);
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int offset)
        {
            yield return string.Format("http://www.ezrss.it/search/index.php?show_name={0}&season={1}&mode=rss", seriesTitle, seasonNumber);

        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, int tvRageId, DateTime date)
        {
            //EZTV doesn't support searching based on actual epidose airdate. they only support release date.
            return new string[0];
        }
    }
}