using Microsoft.AspNetCore.Authorization;

using NzbDrone.Core.Authentication;

namespace Sonarr.Http.Authentication
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public SonarrRole Minimum { get; }

        public PermissionRequirement(SonarrRole minimum)
        {
            Minimum = minimum;
        }
    }
}
