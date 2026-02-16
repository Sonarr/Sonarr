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
            var link = _settings.BaseUrl.Trim();

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

                    link += $"/users/{userName}/watchlist/shows/{watchSorting}";
                    break;
                case (int)TraktUserListType.UserWatchedList:
                    link += $"/users/{userName}/watched/shows";
                    break;
                case (int)TraktUserListType.UserCollectionList:
                    link += $"/users/{userName}/collection/shows";
                    break;
            }

            var filterParams = TraktQueryHelper.BuildFilterParameters(_settings.Rating, _settings.Genres, _settings.Years, _settings.Limit, _settings.TraktAdditionalParameters);

            // Add extended parameter for watched list
            if (_settings.TraktListType == (int)TraktUserListType.UserWatchedList)
            {
                filterParams["extended"] = "full";
            }

            link += "?" + filterParams.ToQueryString();

            var request = new ImportListRequest(link, HttpAccept.Json);

            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", _clientId);

            if (_settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                request.HttpRequest.Headers.Add("Authorization", $"Bearer {_settings.AccessToken}");
            }

            yield return request;
        }
    }
}
