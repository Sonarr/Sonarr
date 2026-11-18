using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Authentication
{
    public class OidcLogout : ModelBase
    {
        public string SessionId { get; set; }
        public string Subject { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
