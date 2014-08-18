using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class Newznab : IndexerBase<NewznabSettings>
    {
        private readonly IFetchFeedFromIndexers _feedFetcher;
        private readonly HttpProvider _httpProvider;
        private readonly Logger _logger;
        
        public Newznab(IFetchFeedFromIndexers feedFetcher, HttpProvider httpProvider, Logger logger)
        {
            _feedFetcher = feedFetcher;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        //protected so it can be mocked, but not used for DI
        //TODO: Is there a better way to achieve this?
        protected Newznab()
        {
        }

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

                list.Add(GetDefinition("Nzbs.org", GetSettings("http://nzbs.org", new List<Int32> { 5000 })));
                list.Add(GetDefinition("Nzb.su", GetSettings("https://api.nzb.su", new List<Int32>())));
                list.Add(GetDefinition("Dognzb.cr", GetSettings("https://api.dognzb.cr", new List<Int32>())));
                list.Add(GetDefinition("OZnzb.com", GetSettings("https://www.oznzb.com", new List<Int32>())));
                list.Add(GetDefinition("nzbplanet.net", GetSettings("https://nzbplanet.net", new List<Int32>())));
                list.Add(GetDefinition("NZBgeek", GetSettings("https://api.nzbgeek.info", new List<Int32>())));

                return list;
            }
        }

        public override ProviderDefinition Definition { get; set; }

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

        public override ValidationResult Test()
        {
            var releases = _feedFetcher.FetchRss(this);

            if (releases.Any()) return new ValidationResult();

            try
            {
                var url = RecentFeed.First();
                var xml = _httpProvider.DownloadString(url);

                NewznabPreProcessor.Process(xml, url);
            }
            catch (ApiKeyException)
            {
                _logger.Warn("Indexer returned result for Newznab RSS URL, API Key appears to be invalid");

                var apiKeyFailure = new ValidationFailure("ApiKey", "Invalid API Key");
                return new ValidationResult(new List<ValidationFailure> { apiKeyFailure });
            }
            catch (RequestLimitReachedException)
            {
                _logger.Warn("Request limit reached");
            }
            catch (Exception ex)
            {
                _logger.WarnException("Unable to connect to indexer: " + ex.Message, ex);

                var failure = new ValidationFailure("Url", "Unable to connect to indexer, check the log for more details");
                return new ValidationResult(new List<ValidationFailure> { failure });
            }

            return new ValidationResult();
        }

        private IndexerDefinition GetDefinition(String name, NewznabSettings settings)
        {
            return new IndexerDefinition
                   {
                       EnableRss = false,
                       EnableSearch = false,
                       Name = name,
                       Implementation = GetType().Name,
                       Settings = settings,
                       Protocol = DownloadProtocol.Usenet,
                       SupportsSearch = SupportsSearch
                   };
        }

        private NewznabSettings GetSettings(String url, List<Int32> categories)
        {
            var settings = new NewznabSettings { Url = url };

            if (categories.Any())
            {
                settings.Categories = categories;
            }

            return settings;
        }

        private static string NewsnabifyTitle(string title)
        {
            return title.Replace("+", "%20");
        }
    }
}
