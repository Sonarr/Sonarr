using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public class TraktUserRequestGenerator : IImportListRequestGenerator
    {
        public TraktUserSettings Settings { get; set; }

        public string ClientId { get; set; }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetSeriesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetSeriesRequest()
        {
            var link = Settings.BaseUrl.Trim();
            var userName = Settings.Username.IsNotNullOrWhiteSpace() ? Settings.Username.Trim() : Settings.AuthUser.Trim();

            switch (Settings.TraktListType)
            {
                case (int)TraktUserListType.UserWatchList:
                    link += $"/users/{userName}/watchlist/shows?limit={Settings.Limit}";
                    break;
                case (int)TraktUserListType.UserWatchedList:
                    link += $"/users/{userName}/watched/shows?limit={Settings.Limit}";
                    break;
                case (int)TraktUserListType.UserCollectionList:
                    link += $"/users/{userName}/collection/shows?limit={Settings.Limit}";
                    break;
            }

            var request = new ImportListRequest(link, HttpAccept.Json);

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
