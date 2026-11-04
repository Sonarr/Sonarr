using System.Collections.Generic;
using System.Net;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.MDBList
{
    public class MDBListRequestGenerator : IImportListRequestGenerator
    {
        public const int Limit = 1000;

        public MDBListSettings Settings { get; set; }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetSeriesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetSeriesRequest()
        {
            var request = new HttpRequestBuilder(Settings.BaseUrl.TrimEnd('/'))
                .Resource($"{MDBListSettings.GetListPath(Settings.ListId)}/items")
                .AddQueryParam("mediatype", "show")
                .AddQueryParam("limit", Limit)
                .Accept(HttpAccept.Json)
                .Build();

            request.Headers.Add("Authorization", "Bearer " + Settings.AccessToken);
            request.SuppressHttpErrorStatusCodes = new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound };

            yield return new ImportListRequest(request);
        }
    }
}
