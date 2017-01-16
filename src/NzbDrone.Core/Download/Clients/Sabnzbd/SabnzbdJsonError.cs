using System;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdJsonError
    {
        public string Status { get; set; }
        public string Error { get; set; }

        public bool Failed => !string.IsNullOrWhiteSpace(Status) &&
                              Status.Equals("false", StringComparison.InvariantCultureIgnoreCase);
    }
}
