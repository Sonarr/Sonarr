using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Simkl.User
{
    public class SimklUserRequestGenerator : IImportListRequestGenerator
    {
        public SimklUserSettings Settings { get; set; }

        public string ClientId { get; set; }

        public SimklUserRequestGenerator()
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
            var link = $"{Settings.BaseUrl.Trim()}/sync/all-items/shows/{((SimklUserListType)Settings.ListType).ToString().ToLowerInvariant()}";

            var request = new ImportListRequest(link, HttpAccept.Json);

            request.HttpRequest.Headers.Add("simkl-api-key", ClientId);

            if (Settings.AccessToken.IsNotNullOrWhiteSpace())
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + Settings.AccessToken);
            }

            yield return request;
        }
    }
}
