using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Indexers
{
    public class IndexerRequest : HttpRequest
    {
        public IndexerRequest(String url)
            : base(url)
        {
        }
    }

    public class IndexerResponse : HttpResponse
    {
        public IndexerResponse(HttpResponse response)
            : base(response.Request, response.Headers, response.ResponseData, response.StatusCode)
        {

        }
    }
}
