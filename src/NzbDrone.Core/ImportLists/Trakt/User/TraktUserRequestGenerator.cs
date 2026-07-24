using System;
using System.Collections.Generic;
using System.Globalization;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public class TraktUserRequestGenerator : IImportListRequestGenerator
    {
        private readonly TraktUserSettings _settings;
        private readonly string _clientId;
        private readonly int _pageSize;
        private readonly int _maxNumResults;

        public TraktUserRequestGenerator(TraktUserSettings settings, string clientId, int pageSize, int maxNumResults)
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

            requestBuilder
                .Accept(HttpAccept.Json)
                .SetHeader("trakt-api-version", "2")
                .SetHeader("trakt-api-key", _clientId);

            if (_settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                requestBuilder.SetHeader("Authorization", $"Bearer {_settings.AccessToken}");
            }

            var userName = _settings.Username.IsNotNullOrWhiteSpace() ? _settings.Username.Trim() : _settings.AuthUser.Trim();

            switch (_settings.TraktListType)
            {
                case (int)TraktUserListType.UserWatchList:
                    var watchSorting = _settings.TraktWatchSorting switch
                    {
                        (int)TraktUserWatchSorting.Added => "added",
                        (int)TraktUserWatchSorting.Title => "title",
                        (int)TraktUserWatchSorting.Released => "released",
                        _ => "rank"
                    };

                    requestBuilder
                        .Resource("/users/{userName}/watchlist/shows/{watchSorting}")
                        .SetSegment("userName", userName)
                        .SetSegment("watchSorting", watchSorting);
                    break;
                case (int)TraktUserListType.UserWatchedList:
                    requestBuilder
                        .Resource("/users/{userName}/watched/shows")
                        .SetSegment("userName", userName);
                    break;
                case (int)TraktUserListType.UserCollectionList:
                    requestBuilder
                        .Resource("/users/{userName}/collection/shows")
                        .SetSegment("userName", userName);
                    break;
            }

            var filterParams = TraktQueryHelper.BuildFilterParameters(_settings.Rating, _settings.Genres, _settings.Years, _pageSize, _settings.TraktAdditionalParameters);

            if (_settings.TraktListType == (int)TraktUserListType.UserWatchedList)
            {
                filterParams["extended"] = "full";
            }

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
