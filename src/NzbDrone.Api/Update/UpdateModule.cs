using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Newtonsoft.Json;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.REST;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Update;
using NzbDrone.Api.Mapping;

namespace NzbDrone.Api.Update
{
    public class UpdateModule : NzbDroneRestModule<UpdateResource>
    {
        private readonly IRecentUpdateProvider _recentUpdateProvider;
        private readonly IInstallUpdates _installUpdateService;

        public UpdateModule(IRecentUpdateProvider recentUpdateProvider,
                            IInstallUpdates installUpdateService)
        {
            _recentUpdateProvider = recentUpdateProvider;
            _installUpdateService = installUpdateService;
            GetResourceAll = GetRecentUpdates;
        }

        private List<UpdateResource> GetRecentUpdates()
        {
            var resources = _recentUpdateProvider.GetRecentUpdatePackages()
                                                 .OrderByDescending(u => u.Version)
                                                 .InjectTo<List<UpdateResource>>();

            foreach (var updateResource in resources)
            {
                if (updateResource.Version > BuildInfo.Version)
                {
                    updateResource.IsUpgrade = true;
                }

                else if (updateResource.Version == BuildInfo.Version)
                {
                    updateResource.Installed = true;
                }
            }

            return resources;
        }
    }

    public class UpdateResource : RestResource
    {
        [JsonConverter(typeof(Newtonsoft.Json.Converters.VersionConverter))]
        public Version Version { get; set; }

        public String Branch { get; set; }
        public DateTime ReleaseDate { get; set; }
        public String FileName { get; set; }
        public String Url { get; set; }
        public Boolean IsUpgrade { get; set; }
        public Boolean Installed { get; set; }
        public UpdateChanges Changes { get; set; }
    }
}