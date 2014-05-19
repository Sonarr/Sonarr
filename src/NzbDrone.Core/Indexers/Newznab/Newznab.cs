using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using NzbDrone.Common;
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
                var categories = String.Join(",", Settings.Categories.Concat(Settings.AnimeCategories));

                var url = String.Format("{0}/api?t=tvsearch&cat={1}&extended=1{2}", Settings.Url.TrimEnd('/'), categories, Settings.AdditionalParameters);

                if (!String.IsNullOrWhiteSpace(Settings.ApiKey))
                {
                    url += "&apikey=" + Settings.ApiKey;
                }

                yield return url;
            }
        }

        public override IEnumerable<string> GetEpisodeSearchUrls(List<String> titles, int tvRageId, int seasonNumber, int episodeNumber)
        {
            if (Settings.Categories.Empty())
            {
                return Enumerable.Empty<String>();
            }

            if (tvRageId > 0)
            {
                return RecentFeed.Select(url => String.Format("{0}&limit=100&rid={1}&season={2}&ep={3}", url, tvRageId, seasonNumber, episodeNumber));
            }

            return titles.SelectMany(title =>
                        RecentFeed.Select(url =>
                                String.Format("{0}&limit=100&q={1}&season={2}&ep={3}",
                                url, NewsnabifyTitle(title), seasonNumber, episodeNumber)));
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(List<String> titles, int tvRageId, DateTime date)
        {
            if (Settings.Categories.Empty())
            {
                return Enumerable.Empty<String>();
            }

            if (tvRageId > 0)
            {
                return RecentFeed.Select(url => String.Format("{0}&limit=100&rid={1}&season={2:yyyy}&ep={2:MM}/{2:dd}", url, tvRageId, date)).ToList();
            }

            return titles.SelectMany(title => 
                        RecentFeed.Select(url =>
                                String.Format("{0}&limit=100&q={1}&season={2:yyyy}&ep={2:MM}/{2:dd}",
                                url, NewsnabifyTitle(title), date)).ToList());
        }

        public override IEnumerable<string> GetAnimeEpisodeSearchUrls(List<String> titles, int tvRageId, int absoluteEpisodeNumber)
        {
            if (Settings.AnimeCategories.Empty())
            {
                return Enumerable.Empty<String>();
            }

            return titles.SelectMany(title =>
                        RecentFeed.Select(url =>
                                String.Format("{0}&limit=100&q={1}+{2:00}",
                                url.Replace("t=tvsearch", "t=search"), NewsnabifyTitle(title), absoluteEpisodeNumber)));
        }

        public override IEnumerable<string> GetSeasonSearchUrls(List<String> titles, int tvRageId, int seasonNumber, int offset)
        {
            if (Settings.Categories.Empty())
            {
                return Enumerable.Empty<String>();
            }

            if (tvRageId > 0)
            {
                return RecentFeed.Select(url => String.Format("{0}&limit=100&rid={1}&season={2}&offset={3}", url, tvRageId, seasonNumber, offset));
            }

            return titles.SelectMany(title =>
                        RecentFeed.Select(url =>
                                String.Format("{0}&limit=100&q={1}&season={2}&offset={3}",
                                url, NewsnabifyTitle(title), seasonNumber, offset)));
        }

        public override IEnumerable<string> GetSearchUrls(string query, int offset)
        {
            // encode query (replace the + with spaces first)
            query = query.Replace("+", " ");
            query = System.Web.HttpUtility.UrlEncode(query);
            return RecentFeed.Select(url => String.Format("{0}&offset={1}&limit=100&q={2}", url.Replace("t=tvsearch", "t=search"), offset, query));
        }

        private static string NewsnabifyTitle(string title)
        {
            return title.Replace("+", "%20");
        }
    }
}
