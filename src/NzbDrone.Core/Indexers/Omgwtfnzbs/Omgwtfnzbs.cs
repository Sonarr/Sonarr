using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class Omgwtfnzbs : IndexerBase<OmgwtfnzbsSettings>
    {
        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Usenet;
            }
        }

        public override IParseFeed Parser
        {
            get
            {
                return new OmgwtfnzbsParser();
            }
        }

        public override IEnumerable<string> RecentFeed
        {
            get
            {

                yield return String.Format("http://rss.omgwtfnzbs.org/rss-search.php?catid=19,20&user={0}&api={1}&eng=1",
                                  Settings.Username, Settings.ApiKey);
            }
        }

        public override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int episodeNumber)
        {
            var searchUrls = new List<string>();

            foreach (var url in RecentFeed)
            {
                searchUrls.Add(String.Format("{0}&search={1}+S{2:00}E{3:00}", url, seriesTitle, seasonNumber, episodeNumber));
            }

            return searchUrls;
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, int tvRageId, DateTime date)
        {
            var searchUrls = new List<String>();

            foreach (var url in RecentFeed)
            {
                searchUrls.Add(String.Format("{0}&search={1}+{2:yyyy MM dd}", url, seriesTitle, date));
            }

            return searchUrls;
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int offset)
        {
            var searchUrls = new List<String>();

            foreach (var url in RecentFeed)
            {
                searchUrls.Add(String.Format("{0}&search={1}+S{2:00}", url, seriesTitle, seasonNumber));
            }

            return searchUrls;
        }

        public override IEnumerable<string> GetSearchUrls(string query, int offset, int limit)
        {
            return new List<string>();
        }

    }
}
