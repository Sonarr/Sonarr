using System;

namespace NzbDrone.Core.Authentication
{
    public enum AuthenticationType
    {
        None = 0,
        [Obsolete("Use Forms authentication instead")]
        Basic = 1,
        Forms = 2,
        External = 3
    }
}
