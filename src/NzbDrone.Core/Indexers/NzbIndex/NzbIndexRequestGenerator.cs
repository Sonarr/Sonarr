using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.NzbIndex
{
    public class NzbIndexRequestGenerator : IIndexerRequestGenerator
    {
        public NzbIndexSettings Settings { get; set; }

        public int MinSize { get; set; }
        public int MaxAge { get; set; }
        public int DefaultMaxSize { get; set; }
        public int RecentRequestMaxAge { get; set; }

        public NzbIndexRequestGenerator(NzbIndexSettings settings)
        {
            Settings = settings;
            DefaultMaxSize = 3000;
            MaxAge = 0;
            RecentRequestMaxAge = 3;
            MinSize = 200;
        }

        public IList<IEnumerable<IndexerRequest>> GetRecentRequests()
        {
            return new List<IEnumerable<IndexerRequest>>
            {
                new List<IndexerRequest> {GetRequest("720p+S0", RecentRequestMaxAge, MinSize, DefaultMaxSize, Settings.ResponseMaxSize)}
            };
        }

        private string GetQParam(string paramName, object paramValue)
        {
            if (paramName.IsNullOrWhiteSpace() || paramValue == null || paramValue.ToString().IsNullOrWhiteSpace() || paramValue.ToString() == "0") return null;

            return string.Format(CultureInfo.InvariantCulture, "{0}={1}", paramName, paramValue);
        }

        private IndexerRequest GetRequest(string searchString, int maxAge, int minSize, int maxSize, int responseMaxSize)
        {
            return new IndexerRequest(BuildRequestUrl(searchString,maxAge, minSize, maxSize, responseMaxSize), HttpAccept.Rss);
        }

        private string BuildRequestUrl(string searchString, int maxAge, int minSize, int maxSize, int responseMaxSize)
        {
            var qsList = new List<string>();
            qsList.AddIfNotNull(GetQParam(Settings.QueryParam, searchString));
            qsList.AddIfNotNull(GetQParam(Settings.MinSizeParam, minSize));
            qsList.AddIfNotNull(GetQParam(Settings.MaxSizeParam, maxSize));
            qsList.AddIfNotNull(GetQParam(Settings.MaxAgeParam, maxAge));
            qsList.AddIfNotNull(GetQParam(Settings.ResponseMaxSizeParam, responseMaxSize));
            if (!string.IsNullOrWhiteSpace(Settings.AdditionalParameters))
                qsList.AddIfNotNull(Settings.AdditionalParameters.TrimStart('&'));

            return string.Format(CultureInfo.InvariantCulture, "{0}?{1}", Settings.Url.TrimEnd('/'), string.Join("&", qsList));
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            return
                new List<IEnumerable<IndexerRequest>>
                {
                    searchCriteria.QueryTitles.Select(
                        queryTitle =>
                            GetRequest(
                                String.Format("{0}+S{1:00}E{2:00}", queryTitle.Replace(" ", "+"),
                                    searchCriteria.SeasonNumber, searchCriteria.EpisodeNumber),
                                MaxAge, MinSize, DefaultMaxSize, Settings.ResponseMaxSize)).ToList()
                };
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var requests = new List<IEnumerable<IndexerRequest>>();
            requests.Add(
            searchCriteria.QueryTitles.Select(queryTitle =>
                GetRequest(String.Format("{0}+S{1:00}", queryTitle.Replace(" ", "+"), searchCriteria.SeasonNumber), MaxAge, MinSize, DefaultMaxSize, Settings.ResponseMaxSize)));

            requests.Add(
            searchCriteria.QueryTitles.Select(queryTitle =>
                GetRequest(String.Format("{0}+Season+{1}", queryTitle.Replace(" ", "+"), searchCriteria.SeasonNumber), MaxAge, MinSize, DefaultMaxSize, Settings.ResponseMaxSize)));
            return requests;
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            return
                new List<IEnumerable<IndexerRequest>>
                {
                    searchCriteria.QueryTitles.Select(
                        queryTitle =>
                            GetRequest(
                                String.Format("{0}+{1:yyyy}+{1:MM}+{1:dd}", queryTitle.Replace(" ", "+"),
                                    searchCriteria.AirDate),
                                MaxAge, MinSize, DefaultMaxSize, Settings.ResponseMaxSize)).ToList()
                };
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            return
                new List<IEnumerable<IndexerRequest>>
                {
                    searchCriteria.QueryTitles.Select(
                        queryTitle =>
                            GetRequest(
                                String.Format("{0}+{1:00}", queryTitle.Replace(" ", "+"),
                                    searchCriteria.AbsoluteEpisodeNumber),
                                MaxAge, MinSize, DefaultMaxSize, Settings.ResponseMaxSize)).ToList()
                };
        }

        public IList<IEnumerable<IndexerRequest>> GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return searchCriteria.QueryTitles.Select(queryTitle =>
                searchCriteria.EpisodeQueryTitles.Select(
                    episodeTitle =>
                        GetRequest(
                            String.Format("{0}+{1}", queryTitle.Replace(" ", "+"), episodeTitle.Replace(" ", "+")),
                            MaxAge, MinSize, DefaultMaxSize, Settings.ResponseMaxSize))
                ).ToList();
        }
    }
}
