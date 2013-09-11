using System;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.Update
{
    public interface IUpdatePackageProvider
    {
        UpdatePackage GetLatestUpdate(string branch, Version currentVersion);
    }

    public class UpdatePackageProvider : IUpdatePackageProvider
    {
        public UpdatePackage GetLatestUpdate(string branch, Version currentVersion)
        {
            var restClient = new RestClient(Services.RootUrl);

            var request = new RestRequest("/v1/update/{branch}");

            request.AddParameter("version", currentVersion);
            request.AddUrlSegment("branch", branch);

            var update = restClient.ExecuteAndValidate<UpdatePackageAvailable>(request);

            if (!update.Available) return null;

            return update.UpdatePackage;
        }
    }
}