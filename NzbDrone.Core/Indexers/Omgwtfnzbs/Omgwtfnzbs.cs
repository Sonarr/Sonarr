using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class Omgwtfnzbs : IndexerWithSetting<OmgwtfnzbsSetting>
    {
        public override string Name
        {
            get { return "omgwtfnzbs"; }
        }

        public override IEnumerable<string> RecentFeed
        {
            get
            {

                yield return String.Format("http://rss.omgwtfnzbs.org/rss-search.php?catid=19,20&user={0}&api={1}&eng=1",
                                  Settings.Username, Settings.ApiKey);
            }
        }

        public override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            var searchUrls = new List<string>();

            foreach (var url in RecentFeed)
            {
                searchUrls.Add(String.Format("{0}&search={1}+S{2:00}E{3:00}", url, seriesTitle, seasonNumber, episodeNumber));
            }

            return searchUrls;
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            var searchUrls = new List<String>();

            foreach (var url in RecentFeed)
            {
                searchUrls.Add(String.Format("{0}&search={1}+{2:yyyy MM dd}", url, seriesTitle, date));
            }

            return searchUrls;
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            var searchUrls = new List<String>();

            foreach (var url in RecentFeed)
            {
                searchUrls.Add(String.Format("{0}&search={1}+S{2:00}", url, seriesTitle, seasonNumber));
            }

            return searchUrls;
        }

        public override IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            var searchUrls = new List<String>();

            foreach (var url in RecentFeed)
            {
                searchUrls.Add(String.Format("{0}&search={1}+S{2:00}E{3}", url, seriesTitle, seasonNumber, episodeWildcard));
            }

            return searchUrls;
        }
    }
}
