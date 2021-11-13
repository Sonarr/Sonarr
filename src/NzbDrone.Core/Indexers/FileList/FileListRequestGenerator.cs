using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.FileList
{
    public class FileListRequestGenerator : IIndexerRequestGenerator
    {
        public FileListSettings Settings { get; set; }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetRequest("latest-torrents", Settings.Categories.Concat(Settings.AnimeCategories), ""));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddImdbRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&season={searchCriteria.SeasonNumber}&episode={searchCriteria.EpisodeNumber}");
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&season={searchCriteria.SeasonNumber}&episode={searchCriteria.EpisodeNumber}");
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&season={searchCriteria.SeasonNumber}&episode={searchCriteria.EpisodeNumber}");
            }

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchID) || searchCriteria.SearchMode == SearchMode.Default)
            {
                AddImdbRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&season={searchCriteria.SeasonNumber}");
            }

            if (searchCriteria.SearchMode.HasFlag(SearchMode.SearchTitle))
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&season={searchCriteria.SeasonNumber}");
            }

            pageableRequests.AddTier();

            if (searchCriteria.SearchMode == SearchMode.Default)
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.Categories, $"&season={searchCriteria.SeasonNumber}");
            }

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public IndexerPageableRequestChain GetSearchRequests(DailySeasonSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            // FileList has absolute releases in E01 format but also release sin S01E01 format, likely by imdb numbering but we only have tvdb numbering... so we try those as fallback to abs.
            AddImdbRequests(pageableRequests, searchCriteria, "search-torrents", Settings.AnimeCategories, $"&season=0&episode={searchCriteria.AbsoluteEpisodeNumber}");
            pageableRequests.AddTier();
            foreach (var eps in searchCriteria.Episodes)
            {
                AddImdbRequests(pageableRequests, searchCriteria, "search-torrents", Settings.AnimeCategories, $"&season={eps.SeasonNumber}&episode={eps.EpisodeNumber}");
            }

            pageableRequests.AddTier();

            AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.AnimeCategories, $"&season=0&episode={searchCriteria.AbsoluteEpisodeNumber}");
            pageableRequests.AddTier();
            foreach (var eps in searchCriteria.Episodes)
            {
                AddNameRequests(pageableRequests, searchCriteria, "search-torrents", Settings.AnimeCategories, $"&season={eps.SeasonNumber}&episode={eps.EpisodeNumber}");
            }

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        private void AddImdbRequests(IndexerPageableRequestChain chain, SearchCriteriaBase searchCriteria, string searchType, IEnumerable<int> categories, string parameters)
        {
            if (searchCriteria.Series.ImdbId.IsNotNullOrWhiteSpace())
            {
                chain.Add(GetRequest(searchType, categories, string.Format("&type=imdb&query={0}{1}", searchCriteria.Series.ImdbId, parameters)));
            }
        }

        private void AddNameRequests(IndexerPageableRequestChain chain, SearchCriteriaBase searchCriteria, string searchType, IEnumerable<int> categories, string parameters)
        {
            foreach (var sceneTitle in searchCriteria.SceneTitles)
            {
                chain.Add(GetRequest(searchType, categories, string.Format("&type=name&query={0}{1}", Uri.EscapeDataString(sceneTitle.Trim()), parameters)));
            }
        }

        private IEnumerable<IndexerRequest> GetRequest(string searchType, IEnumerable<int> categories, string parameters)
        {
            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl = string.Format("{0}/api.php?action={1}&category={2}{3}", Settings.BaseUrl.TrimEnd('/'), searchType, categoriesQuery, parameters);

            var request = new IndexerRequest(baseUrl, HttpAccept.Json);
            request.HttpRequest.Credentials = new BasicNetworkCredential(Settings.Username.Trim(), Settings.Passkey.Trim());

            yield return request;
        }
    }
}
