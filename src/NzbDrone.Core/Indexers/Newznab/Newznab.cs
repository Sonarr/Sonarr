using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class Newznab : IndexerBase<NewznabSettings>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Usenet; } }
        public override Int32 SupportedPageSize { get { return 100; } }

        public override IParseFeed Parser
        {
            get
            {
                return new NewznabParser();
            }
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
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
                    Settings = GetSettings("https://api.nzb.su", new List<Int32>())
                });

                list.Add(new IndexerDefinition
                {
                    Enable = false,
                    Name = "Dognzb.cr",
                    Implementation = GetType().Name,
                    Settings = GetSettings("https://api.dognzb.cr", new List<Int32>())
                });

                list.Add(new IndexerDefinition
                {
                    Enable = false,
                    Name = "OZnzb.com",
                    Implementation = GetType().Name,
                    Settings = GetSettings("https://www.oznzb.com", new List<Int32>())
                });

                return list;
            }
        }

        public override ProviderDefinition Definition { get; set; }

        private NewznabSettings GetSettings(string url, List<int> categories)
        {
            var settings = new NewznabSettings { Url = url };

            if (categories.Any())
            {
                settings.Categories = categories;
            }

            return settings;
        }

        public override IEnumerable<string> RecentFeed
        {
            get
            {
                var url = String.Format("{0}/api?t=tvsearch&cat={1}&extended=1{2}", Settings.Url.TrimEnd('/'), String.Join(",", Settings.Categories), Settings.AdditionalParameters);

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

        public override IEnumerable<string> GetSearchUrls(string query, int offset)
        {
            // encode query (replace the + with spaces first)
            query = query.Replace("+", " ");
            query = System.Web.HttpUtility.UrlEncode(query);
            return RecentFeed.Select(url => String.Format("{0}&offset={1}&limit=100&q={2}", url.Replace("t=tvsearch", "t=search"), offset, query));
        }


        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, int tvRageId, DateTime date)
        {
            if (tvRageId > 0)
            {
                return RecentFeed.Select(url => String.Format("{0}&limit=100&rid={1}&season={2:yyyy}&ep={2:MM}/{2:dd}", url, tvRageId, date)).ToList();
            }

            return RecentFeed.Select(url => String.Format("{0}&limit=100&q={1}&season={2:yyyy}&ep={2:MM}/{2:dd}", url, NewsnabifyTitle(seriesTitle), date)).ToList();
        }

        public override IEnumerable<string> GetAnimeEpisodeSearchUrls(string seriesTitle, int tvRageId, int absoluteEpisodeNumber)
        {
            /*
             * This is going to be interesting because often the tvRageId doesn't always 
             * match the series and when there are multiple names by season its a mess
             * For now: just search based on name and abs episode number
             */

            return RecentFeed.Select(url => String.Format("{0}&limit=100&q={1}+{2}", url.Replace("t=tvsearch", "t=search"), NewsnabifyTitle(seriesTitle), absoluteEpisodeNumber));
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int tvRageId, int seasonNumber, int offset)
        {
            if (tvRageId > 0)
            {
                return RecentFeed.Select(url => String.Format("{0}&limit=100&rid={1}&season={2}&offset={3}", url, tvRageId, seasonNumber, offset));
            }

            return RecentFeed.Select(url => String.Format("{0}&limit=100&q={1}&season={2}&offset={3}", url, NewsnabifyTitle(seriesTitle), seasonNumber, offset));
        }

        private static string NewsnabifyTitle(string title)
        {
            return title.Replace("+", "%20");
        }
    }
}
