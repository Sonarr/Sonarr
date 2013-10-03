using System;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class JsonError
    {
        public String Version { get; set; }
        public ErrorModel Error { get; set; }
    }
}
