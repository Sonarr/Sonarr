using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Organizer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Indexers.AirDCPP
{
    public class AirDCPPRequestGenerator : IIndexerRequestGenerator
    {
        protected readonly IHttpClient _httpClient;
        protected readonly IAirDCPPProxy _airDCPPProxy;

        public AirDCPPRequestGenerator(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;

            _airDCPPProxy = new AirDCPPProxy(_httpClient, logger);
        }

        public AirDCPPSettings Settings { get; set; }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest("a"));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            return GetDefaultSearchRequest(searchCriteria.SceneTitles, searchCriteria.Episodes.Select(e => (e.SeasonNumber, e.EpisodeNumber)).ToList());
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            // Search for seasons is currently not supported by AirDC++. Let's search for all episodes.
            return GetDefaultSearchRequest(searchCriteria.SceneTitles, searchCriteria.Episodes.Select(e => (e.SeasonNumber, e.EpisodeNumber)).ToList());
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            return GetDefaultSearchRequest(searchCriteria.SceneTitles, searchCriteria.Episodes.Select(e => (e.SeasonNumber, e.EpisodeNumber)).ToList());
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
        {
            // Search for seasons is currently not supported by AirDC++. Let's search for all episodes.
            return GetDefaultSearchRequest(searchCriteria.SceneTitles, searchCriteria.Episodes.Select(e => (e.SeasonNumber, e.EpisodeNumber)).ToList());
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            return GetDefaultSearchRequest(searchCriteria.SceneTitles, searchCriteria.Episodes.Select(e => (e.SeasonNumber, e.EpisodeNumber)).ToList());
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return GetDefaultSearchRequest(searchCriteria.SceneTitles, searchCriteria.Episodes.Select(e => (e.SeasonNumber, e.EpisodeNumber)).ToList());
        }

        private IndexerPageableRequestChain GetDefaultSearchRequest(List<string> seriesTitles, List<(int season, int episode)> episodes)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var seriesTitle in seriesTitles)
            {
                foreach (var episode in episodes)
                {
                    var searchString = string.Format("{0} S{1:00}E{2:00}", seriesTitle, episode.season, episode.episode);
                    pageableRequests.Add(GetRequest(FileNameBuilder.CleanFileName(searchString)));
                }
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchName)
        {
            var request = _airDCPPProxy.PerformSearch(Settings, searchName);
            yield return new IndexerRequest(request);
        }
    }
}
