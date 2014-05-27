using System.Collections.Generic;
using System.Linq;
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
}