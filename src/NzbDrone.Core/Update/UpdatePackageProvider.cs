using System;
using System.Collections.Generic;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.Update
{
    public interface IUpdatePackageProvider
    {
        UpdatePackage GetLatestUpdate(string branch, Version currentVersion);
        List<UpdatePackage> GetRecentUpdates(string branch);
    }

    public class UpdatePackageProvider : IUpdatePackageProvider
    {
        public UpdatePackage GetLatestUpdate(string branch, Version currentVersion)
        {
            var restClient = new RestClient(Services.RootUrl);

            var request = new RestRequest("/v1/update/{branch}");

            request.AddParameter("version", currentVersion);
            request.AddParameter("os", OsInfo.Os.ToString().ToLowerInvariant());
            request.AddUrlSegment("branch", branch);

            var update = restClient.ExecuteAndValidate<UpdatePackageAvailable>(request);

            if (!update.Available) return null;

            return update.UpdatePackage;
        }

        public List<UpdatePackage> GetRecentUpdates(string branch)
        {
            var restClient = new RestClient(Services.RootUrl);

            var request = new RestRequest("/v1/update/{branch}/changes");

            request.AddParameter("majorVersion", BuildInfo.Version.Major);
            request.AddParameter("os", OsInfo.Os.ToString().ToLowerInvariant());
            request.AddUrlSegment("branch", branch);

            var updates = restClient.ExecuteAndValidate<List<UpdatePackage>>(request);

            return updates;
        }
    }
}