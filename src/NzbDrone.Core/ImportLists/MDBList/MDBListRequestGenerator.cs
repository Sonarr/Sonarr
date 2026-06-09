using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.MDBList
{
    public class MDBListRequestGenerator : IImportListRequestGenerator
    {
        private const int PageSize = 1000;
        private const int MaxPages = 10;

        public MDBListSettings Settings { get; set; }

        public virtual ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetSeriesRequests());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetSeriesRequests()
        {
            var list = MDBListSettings.ParseListUrl(Settings.ListUrl);

            for (var page = 0; page < MaxPages; page++)
            {
                var requestBuilder = new HttpRequestBuilder(Settings.BaseUrl.Trim())
                    .Resource("/lists/{username}/{listname}/items")
                    .SetSegment("username", Uri.EscapeDataString(list.Username))
                    .SetSegment("listname", Uri.EscapeDataString(list.ListName))
                    .AddQueryParam("apikey", Settings.ApiKey.Trim())
                    .AddQueryParam("limit", PageSize)
                    .AddQueryParam("offset", page * PageSize)
                    .AddQueryParam("mediatype", "show")
                    .Accept(HttpAccept.Json);

                yield return new ImportListRequest(requestBuilder.Build());
            }
        }
    }
}
