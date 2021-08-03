using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Update;
using NzbDrone.Core.Update.History;
using Sonarr.Http;

namespace Sonarr.Api.V3.Update
{
    public class UpdateModule : SonarrRestModule<UpdateResource>
    {
        private readonly IRecentUpdateProvider _recentUpdateProvider;
        private readonly IUpdateHistoryService _updateHistoryService;

        public UpdateModule(IRecentUpdateProvider recentUpdateProvider, IUpdateHistoryService updateHistoryService)
        {
            _recentUpdateProvider = recentUpdateProvider;
            _updateHistoryService = updateHistoryService;
            GetResourceAll = GetRecentUpdates;
        }

        private List<UpdateResource> GetRecentUpdates()
        {
            var resources = _recentUpdateProvider.GetRecentUpdatePackages()
                                                 .OrderByDescending(u => u.Version)
                                                 .ToResource();

            if (resources.Any())
            {
                var first = resources.First();
                first.Latest = true;

                if (first.Version > BuildInfo.Version)
                {
                    first.Installable = true;
                }

                var installed = resources.SingleOrDefault(r => r.Version == BuildInfo.Version);

                if (installed != null)
                {
                    installed.Installed = true;
                }

                var installDates = _updateHistoryService.InstalledSince(resources.Last().ReleaseDate)
                                                        .DistinctBy(v => v.Version)
                                                        .ToDictionary(v => v.Version);

                foreach (var resource in resources)
                {
                    if (installDates.TryGetValue(resource.Version, out var installDate))
                    {
                        resource.InstalledOn = installDate.Date;
                    }
                }
            }

            return resources;
        }
    }
}
