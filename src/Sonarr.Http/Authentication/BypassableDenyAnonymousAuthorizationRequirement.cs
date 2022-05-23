using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace NzbDrone.Http.Authentication
{
    public class BypassableDenyAnonymousAuthorizationRequirement : DenyAnonymousAuthorizationRequirement
    {
    }
}
