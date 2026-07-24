using System;
using System.Collections.Generic;
using System.Globalization;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt.Popular
{
    public class TraktPopularRequestGenerator : IImportListRequestGenerator
    {
        private readonly TraktPopularSettings _settings;
        private readonly string _clientId;
        private readonly int _pageSize;
        private readonly int _maxNumResults;

        public TraktPopularRequestGenerator(TraktPopularSettings settings, string clientId, int pageSize, int maxNumResults)
        {
            _settings = settings;
            _clientId = clientId;
            _pageSize = pageSize;
            _maxNumResults = maxNumResults;
        }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetSeriesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetSeriesRequest()
        {
            var requestBuilder = new HttpRequestBuilder(_settings.BaseUrl.Trim());

            var resource = "/shows";

            switch (_settings.TraktListType)
            {
                case (int)TraktPopularListType.Trending:
                    resource += "/trending";
                    break;
                case (int)TraktPopularListType.Popular:
                    resource += "/popular";
                    break;
                case (int)TraktPopularListType.Anticipated:
                    resource += "/anticipated";
                    break;
                case (int)TraktPopularListType.TopWatchedByWeek:
                    resource += "/watched/weekly";
                    break;
                case (int)TraktPopularListType.TopWatchedByMonth:
                    resource += "/watched/monthly";
                    break;
#pragma warning disable CS0612
                case (int)TraktPopularListType.TopWatchedByYear:
#pragma warning restore CS0612
                    resource += "/watched/yearly";
                    break;
                case (int)TraktPopularListType.TopWatchedByAllTime:
                    resource += "/watched/all";
                    break;
                case (int)TraktPopularListType.RecommendedByWeek:
                    resource += "/recommended/weekly";
                    break;
                case (int)TraktPopularListType.RecommendedByMonth:
                    resource += "/recommended/monthly";
                    break;
#pragma warning disable CS0612
                case (int)TraktPopularListType.RecommendedByYear:
#pragma warning restore CS0612
                    resource += "/recommended/yearly";
                    break;
                case (int)TraktPopularListType.RecommendedByAllTime:
                    resource += "/recommended/all";
                    break;
            }

            requestBuilder
                .Resource(resource)
                .Accept(HttpAccept.Json)
                .SetHeader("trakt-api-version", "2")
                .SetHeader("trakt-api-key", _clientId);

            if (_settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                requestBuilder.SetHeader("Authorization", $"Bearer {_settings.AccessToken}");
            }

            var filterParams = TraktQueryHelper.BuildFilterParameters(_settings.Rating, _settings.Genres, _settings.Years, _pageSize, _settings.TraktAdditionalParameters);

            foreach (var param in filterParams)
            {
                requestBuilder.AddQueryParam(param.Key, param.Value);
            }

            var limit = Math.Clamp(_settings.Limit, 0, _maxNumResults);
            var maxPages = (int)Math.Ceiling(decimal.Divide(limit, _pageSize));
            var remainderLimit = limit % _pageSize;

            for (var page = 1; page <= maxPages; page++)
            {
                if (maxPages == 1 && remainderLimit > 0)
                {
                    requestBuilder.AddQueryParam("limit", remainderLimit.ToString(CultureInfo.InvariantCulture), true);
                }

                requestBuilder.AddQueryParam("page", page.ToString(CultureInfo.InvariantCulture), true);

                yield return new ImportListRequest(requestBuilder.Build());
            }
        }
    }
}
