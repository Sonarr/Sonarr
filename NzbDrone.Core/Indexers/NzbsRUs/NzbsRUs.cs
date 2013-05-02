using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.NzbsRUs
{
    public class Nzbsrus : IndexerWithSetting<NzbsrusSetting>
    {
        public override IEnumerable<string> RecentFeed
        {
            get
            {
                yield return string.Format("https://www.nzbsrus.com/rssfeed.php?cat=91,75&i={0}&h={1}",
                        Settings.Uid,
                        Settings.Hash);

            }
        }

        public override string Name
        {
            get { return "NzbsRUs"; }
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


    }
}