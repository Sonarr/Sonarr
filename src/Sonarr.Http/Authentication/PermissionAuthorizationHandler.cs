using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

using NzbDrone.Core.Authentication;

namespace Sonarr.Http.Authentication
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var hasOidcSub = context.User?.FindFirst("oidc_sub") != null;

            if (!hasOidcSub)
            {
                // Only the authenticated API-key / Forms path may bypass the role
                // check. An unauthenticated principal must NOT succeed here; the
                // policy's RequireAuthenticatedUser handles the 401.
                if (context.User?.Identity?.IsAuthenticated != true)
                {
                    return Task.CompletedTask;
                }

                // Forms/ApiKey path: preserve existing bypass/succeed behavior.
                var roleClaim = context.User?.FindFirst("role")?.Value
                    ?? context.User?.FindFirst("sonarr_role")?.Value;
                if (roleClaim == null)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                var role = SonarrRole.Admin;
                if (Enum.TryParse<SonarrRole>(roleClaim, true, out var parsedRole) && Enum.IsDefined(typeof(SonarrRole), parsedRole))
                {
                    role = parsedRole;
                }

                if (role >= requirement.Minimum)
                {
                    context.Succeed(requirement);
                }

                return Task.CompletedTask;
            }

            // OIDC session: require a valid sonarr_role claim.
            var sonarrRoleClaim = context.User?.FindFirst("sonarr_role")?.Value;
            if (string.IsNullOrWhiteSpace(sonarrRoleClaim) ||
                !Enum.TryParse<SonarrRole>(sonarrRoleClaim, true, out var oidcRole) ||
                !Enum.IsDefined(typeof(SonarrRole), oidcRole))
            {
                return Task.CompletedTask; // deny
            }

            if (oidcRole >= requirement.Minimum)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
