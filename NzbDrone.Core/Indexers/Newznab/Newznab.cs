using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class Newznab : IndexerWithSetting<NewznabSettings>
    {
        public override IParseFeed Parser
        {
            get
            {
                return new NewznabParser();
            }
        }

        public override IEnumerable<IndexerDefinition> DefaultDefinitions
        {
            get
            {
                var list = new List<IndexerDefinition>();

                list.Add(new IndexerDefinition
                {
                    Enable = false,
                    Name = "Nzbs.org",
                    Implementation = GetType().Name,
                    Settings = GetSettings("http://nzbs.org", new List<Int32> { 5000 })
                });


                list.Add(new IndexerDefinition
                {
                    Enable = false,
                    Name = "Nzb.su",
                    Implementation = GetType().Name,
                    Settings = GetSettings("https://nzb.su", new List<Int32>())
                });

                list.Add(new IndexerDefinition
                {
                    Enable = false,
                    Name = "Dognzb.cr",
                    Implementation = GetType().Name,
                    Settings = GetSettings("https://dognzb.cr", new List<Int32>())
                });

                return list;

            }
        }

        private string GetSettings(string url, List<int> categories)
        {
            var settings = new NewznabSettings { Url = url };

            if (categories.Any())
            {
                settings.Categories = categories;
            }

            return settings.ToJson();
        }

        public override IEnumerable<string> RecentFeed
        {
            get
            {
                //Todo: We should be able to update settings on start
                if (Name.Equals("nzbs.org", StringComparison.InvariantCultureIgnoreCase))
                {
                    Settings.Categories = new List<int> { 5000 };
                }

                var url = String.Format("{0}/api?t=tvsearch&cat={1}", Settings.Url.TrimEnd('/'), String.Join(",", Settings.Categories));

                if (!String.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    url += "&apikey=" + Settings.ApiKey;
                }

                yield return url;
            }
        }

        public override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int episodeNumber)
        {
            if (tvRageId > 0)
            {
                return RecentFeed.Select(url => String.Format("{0}&limit=100&rid={1}&season={2}&ep={3}", url, tvRageId, seasonNumber, episodeNumber));
            }

            return RecentFeed.Select(url => String.Format("{0}&limit=100&q={1}&season={2}&ep={3}", url, NewsnabifyTitle(seriesTitle), seasonNumber, episodeNumber));
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, int tvRageId, DateTime date)
        {
            if (tvRageId > 0)
            {
                return RecentFeed.Select(url => String.Format("{0}&limit=100&rid={1}&season={2:yyyy}&ep={2:MM/dd}", url, tvRageId, date)).ToList();
            }

            return RecentFeed.Select(url => String.Format("{0}&limit=100&q={1}&season={2:yyyy}&ep={2:MM/dd}", url, NewsnabifyTitle(seriesTitle), date)).ToList();
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int tvRageId, int seasonNumber)
        {
            if (tvRageId > 0)
            {
                return RecentFeed.Select(url => String.Format("{0}&limit=100&rid={1}&season={2}", url, tvRageId, seasonNumber));
            }

            return RecentFeed.Select(url => String.Format("{0}&limit=100&q={1}&season={2}", url, NewsnabifyTitle(seriesTitle), seasonNumber));
        }

        public override string Name
        {
            get
            {
                return InstanceDefinition.Name;
            }
        }

        private static string NewsnabifyTitle(string title)
        {
            return title.Replace("+", "%20");
        }
    }
}