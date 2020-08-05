using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt.Popular
{
    public class TraktPopularRequestGenerator : IImportListRequestGenerator
    {
        public TraktPopularSettings Settings { get; set; }

        public string ClientId { get; set; }

        public TraktPopularRequestGenerator()
        {
        }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetSeriesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetSeriesRequest()
        {
            var link = Settings.BaseUrl.Trim();

            var filtersAndLimit = $"?years={Settings.Years}&genres={Settings.Genres.ToLower()}&ratings={Settings.Rating}&limit={Settings.Limit}{Settings.TraktAdditionalParameters}";

            switch (Settings.TraktListType)
            {
                case (int)TraktPopularListType.Trending:
                    link += "/shows/trending" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.Popular:
                    link += "/shows/popular" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.Anticipated:
                    link += "/shows/anticipated" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.TopWatchedByWeek:
                    link += "/shows/watched/weekly" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.TopWatchedByMonth:
                    link += "/shows/watched/monthly" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.TopWatchedByYear:
                    link += "/shows/watched/yearly" + filtersAndLimit;
                    break;
                case (int)TraktPopularListType.TopWatchedByAllTime:
                    link += "/shows/watched/all" + filtersAndLimit;
                    break;
            }

            var request = new ImportListRequest($"{link}", HttpAccept.Json);

            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", ClientId);

            if (Settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + Settings.AccessToken);
            }

            yield return request;
        }
    }
}
