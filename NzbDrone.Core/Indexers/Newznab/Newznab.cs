using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class Newznab : IndexerWithSetting<NewznabSettings>
    {
        private readonly IJsonSerializer _jsonSerializer;

        public Newznab()
        {
            _jsonSerializer = new JsonSerializer();
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
                    Settings = GetSettings("http://nzbs.org")
                });


                list.Add(new IndexerDefinition
                {
                    Enable = false,
                    Name = "Nzb.su",
                    Implementation = GetType().Name,
                    Settings = GetSettings("https://nzb.su")
                });

                list.Add(new IndexerDefinition
                {
                    Enable = false,
                    Name = "Dognzb.cr",
                    Implementation = GetType().Name,
                    Settings = GetSettings("https://dognzb.cr")
                });

                return list;

            }
        }

        private string GetSettings(string url)
        {
            return _jsonSerializer.Serialize(new NewznabSettings { Url = url });
        }

        public override IEnumerable<string> RecentFeed
        {
            get
            {
                var url = String.Format("{0}/api?t=tvsearch&cat=5030,5040,5070,5090s", Settings.Url);

                if (String.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    url += "&apikey=" + Settings.ApiKey;
                }

                yield return url;
            }
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