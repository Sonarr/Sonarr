using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class Omgwtfnzbs : IndexerBase<OmgwtfnzbsSettings>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Usenet; } }

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

        public override IEnumerable<string> GetEpisodeSearchUrls(List<String> titles, int tvRageId, int seasonNumber, int episodeNumber)
        {
            var searchUrls = new List<string>();

            foreach (var url in RecentFeed)
            {
                foreach (var title in titles)
                {
                    searchUrls.Add(String.Format("{0}&search={1}+S{2:00}E{3:00}", url, title, seasonNumber, episodeNumber));
                }
            }

            return searchUrls;
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(List<String> titles, int tvRageId, DateTime date)
        {
            var searchUrls = new List<String>();

            foreach (var url in RecentFeed)
            {
                foreach (var title in titles)
                {
                    searchUrls.Add(String.Format("{0}&search={1}+{2:yyyy MM dd}", url, title, date));
                }
            }

            return searchUrls;
        }

        public override IEnumerable<string> GetAnimeEpisodeSearchUrls(List<String> titles, int tvRageId, int absoluteEpisodeNumber)
        {
            // TODO: Implement
            return new List<string>();
        }

        public override IEnumerable<string> GetSeasonSearchUrls(List<String> titles, int tvRageId, int seasonNumber, int offset)
        {
            var searchUrls = new List<String>();

            foreach (var url in RecentFeed)
            {
                foreach (var title in titles)
                {
                    searchUrls.Add(String.Format("{0}&search={1}+S{2:00}", url, title, seasonNumber));
                }
            }

            return searchUrls;
        }

        public override IEnumerable<string> GetSearchUrls(string query, int offset)
        {
            return new List<string>();
        }
    }
}
