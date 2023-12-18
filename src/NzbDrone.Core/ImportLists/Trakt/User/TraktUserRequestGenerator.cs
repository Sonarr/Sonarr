using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public class TraktUserRequestGenerator : IImportListRequestGenerator
    {
        private readonly TraktUserSettings _settings;
        private readonly string _clientId;

        public TraktUserRequestGenerator(TraktUserSettings settings, string clientId)
        {
            _settings = settings;
            _clientId = clientId;
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
                        .Resource("/users/{userName}/watchlist/shows/{sorting}")
                        .SetSegment("sorting", watchSorting);
                    break;
                case (int)TraktUserListType.UserWatchedList:
                    requestBuilder
                        .Resource("/users/{userName}/watched/shows")
                        .AddQueryParam("extended", "full");
                    break;
                case (int)TraktUserListType.UserCollectionList:
                    requestBuilder.Resource("/users/{userName}/collection/shows");
                    break;
            }

            var userName = _settings.Username.IsNotNullOrWhiteSpace() ? _settings.Username.Trim() : _settings.AuthUser.Trim();

            requestBuilder
                .SetSegment("userName", userName)
                .Accept(HttpAccept.Json)
                .WithRateLimit(4)
                .SetHeader("trakt-api-version", "2")
                .SetHeader("trakt-api-key", _clientId)
                .AddQueryParam("limit", _settings.Limit.ToString());

            if (_settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                requestBuilder.SetHeader("Authorization", $"Bearer {_settings.AccessToken}");
            }

            yield return new ImportListRequest(requestBuilder.Build());
        }
    }
}
