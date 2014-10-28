using System;
using System.Collections.Generic;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.Update
{
    public interface IUpdatePackageProvider
    {
        UpdatePackage GetLatestUpdate(string branch, Version currentVersion);
        List<UpdatePackage> GetRecentUpdates(string branch);
    }

    public class UpdatePackageProvider : IUpdatePackageProvider
    {
        private readonly IHttpClient _httpClient;
        private readonly IDroneServicesRequestBuilder _requestBuilder;

        public UpdatePackageProvider(IHttpClient httpClient, IDroneServicesRequestBuilder requestBuilder)
        {
            _httpClient = httpClient;
            _requestBuilder = requestBuilder;
        }

        public UpdatePackage GetLatestUpdate(string branch, Version currentVersion)
        {
            var request = _requestBuilder.Build("/update/{branch}");
            request.UriBuilder.SetQueryParam("version", currentVersion);
            request.UriBuilder.SetQueryParam("os", OsInfo.Os.ToString().ToLowerInvariant());
            request.AddSegment("branch", branch);

            var update = _httpClient.Get<UpdatePackageAvailable>(request).Resource;

            if (!update.Available) return null;

            return update.UpdatePackage;
        }

        public List<UpdatePackage> GetRecentUpdates(string branch)
        {
            var request = _requestBuilder.Build("/update/{branch}/changes");
            request.UriBuilder.SetQueryParam("os", OsInfo.Os.ToString().ToLowerInvariant());
            request.AddSegment("branch", branch);

            var updates = _httpClient.Get<List<UpdatePackage>>(request);

            return updates.Resource;
        }
    }
}