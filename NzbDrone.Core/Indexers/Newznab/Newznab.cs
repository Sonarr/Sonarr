using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class Newznab : Indexer
    {
        private readonly INewznabService _newznabProvider;

        public Newznab(INewznabService newznabProvider)
        {
            _newznabProvider = newznabProvider;
        }


        public override IEnumerable<string> RecentFeed
        {
            get { return GetUrls(); }
        }

        public override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            return RecentFeed.Select(url => String.Format("{0}&limit=100&q={1}&season={2}&ep={3}", url, NewsnabifyTitle(seriesTitle), seasonNumber, episodeNumber));
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            return RecentFeed.Select(url => String.Format("{0}&limit=100&q={1}&season={2:yyyy}&ep={2:MM/dd}", url, NewsnabifyTitle(seriesTitle), date)).ToList();
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            return RecentFeed.Select(url => String.Format("{0}&limit=100&q={1}&season={2}", url, NewsnabifyTitle(seriesTitle), seasonNumber));
        }

        public override IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            return RecentFeed.Select(url => String.Format("{0}&limit=100&q={1}+S{2:00}E{3}", url, NewsnabifyTitle(seriesTitle), seasonNumber, episodeWildcard));
        }

        public override string Name
        {
            get { return "Newznab"; }
        }


        private IEnumerable<string> GetUrls()
        {
            var urls = new List<string>();
            var newznabIndexers = _newznabProvider.Enabled();

            foreach (var newznabDefinition in newznabIndexers)
            {
                var url = String.Format("{0}/api?t=tvsearch&cat=5030,5040,5070,5090s", newznabDefinition.Url);

                if (String.IsNullOrWhiteSpace(newznabDefinition.ApiKey))
                {
                    url += "&apikey=" + newznabDefinition.ApiKey;
                }

                urls.Add(url);
            }

            return urls;
        }


        private static string NewsnabifyTitle(string title)
        {
            return title.Replace("+", "%20");
        }

    }
}