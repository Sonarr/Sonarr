using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt.List
{
    public class TraktListRequestGenerator : IImportListRequestGenerator
    {
        public TraktListSettings Settings { get; set; }
        public string ClientId { get; set; }

        public TraktListRequestGenerator()
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

            link += $"/users/{Settings.Username.Trim()}/lists/{Settings.Listname.ToUrlSlug()}/items/shows?limit={Settings.Limit}";

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
