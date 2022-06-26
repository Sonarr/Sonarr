﻿using Sonarr.Api.V3.Indexers;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class ReleaseClient : ClientBase<ReleaseResource>
    {
        public ReleaseClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey)
        {
        }
    }
}
