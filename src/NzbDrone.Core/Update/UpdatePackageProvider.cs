using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Core.Analytics;

namespace NzbDrone.Core.Update
{
    public interface IUpdatePackageProvider
    {
        UpdatePackage GetLatestUpdate(string branch, Version currentVersion);
        List<UpdatePackage> GetRecentUpdates(string branch, Version currentVersion, Version previousVersion = null);
    }

    public class UpdatePackageProvider : IUpdatePackageProvider
    {
        private readonly IHttpClient _httpClient;
        private readonly IHttpRequestBuilderFactory _requestBuilder;
        private readonly IPlatformInfo _platformInfo;
        private readonly IAnalyticsService _analyticsService;

        public UpdatePackageProvider(IHttpClient httpClient, ISonarrCloudRequestBuilder requestBuilder, IAnalyticsService analyticsService, IPlatformInfo platformInfo)
        {
            _platformInfo = platformInfo;
            _analyticsService = analyticsService;
            _requestBuilder = requestBuilder.Services;
            _httpClient = httpClient;
        }

        public UpdatePackage GetLatestUpdate(string branch, Version currentVersion)
        {
            var request = _requestBuilder.Create()
                                         .Resource("/update/{branch}")
                                         .AddQueryParam("version", currentVersion)
                                         .AddQueryParam("os", OsInfo.Os.ToString().ToLowerInvariant())
                                         .AddQueryParam("arch", RuntimeInformation.OSArchitecture)
                                         .AddQueryParam("runtime", PlatformInfo.Platform.ToString().ToLowerInvariant())
                                         .AddQueryParam("runtimeVer", _platformInfo.Version)
                                         .SetSegment("branch", branch);

            if (_analyticsService.IsEnabled)
            {
                // Send if the system is active so we know which versions to deprecate/ignore
                request.AddQueryParam("active", _analyticsService.InstallIsActive.ToString().ToLower());
            }

            var update = _httpClient.Get<UpdatePackageAvailable>(request.Build()).Resource;

            if (!update.Available)
            {
                return null;
            }

            return update.UpdatePackage;
        }

        public List<UpdatePackage> GetRecentUpdates(string branch, Version currentVersion, Version previousVersion)
        {
            var request = _requestBuilder.Create()
                                         .Resource("/update/{branch}/changes")
                                         .AddQueryParam("version", currentVersion)
                                         .AddQueryParam("os", OsInfo.Os.ToString().ToLowerInvariant())
                                         .AddQueryParam("arch", RuntimeInformation.OSArchitecture)
                                         .AddQueryParam("runtime", PlatformInfo.Platform.ToString().ToLowerInvariant())
                                         .AddQueryParam("runtimeVer", _platformInfo.Version)
                                         .SetSegment("branch", branch);

            if (previousVersion != null && previousVersion != currentVersion)
            {
                request.AddQueryParam("prevVersion", previousVersion);
            }

            if (_analyticsService.IsEnabled)
            {
                // Send if the system is active so we know which versions to deprecate/ignore
                request.AddQueryParam("active", _analyticsService.InstallIsActive.ToString().ToLower());
            }

            var updates = _httpClient.Get<List<UpdatePackage>>(request.Build());

            return updates.Resource;
        }
    }
}
