using System;

namespace NzbDrone.Core.Download.Clients.NzbVortex
{
    public class NzbVortexJsonError
    {
        public string Status { get; set; }
        public string Error { get; set; }

        public bool Failed => !string.IsNullOrWhiteSpace(Status) &&
                              Status.Equals("false", StringComparison.InvariantCultureIgnoreCase);
    }
}
