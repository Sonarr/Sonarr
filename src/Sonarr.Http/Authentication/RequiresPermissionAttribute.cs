using System;

using Microsoft.AspNetCore.Authorization;

using NzbDrone.Core.Authentication;

namespace Sonarr.Http.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequiresPermissionAttribute : AuthorizeAttribute
    {
        public RequiresPermissionAttribute(SonarrRole role)
        {
            Policy = role switch
            {
                SonarrRole.ReadOnly => "UI.Read",
                SonarrRole.AddSearch => "UI.Add",
                SonarrRole.Manage => "UI.Manage",
                SonarrRole.Admin => "UI.Admin",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
            };
        }
    }
}
