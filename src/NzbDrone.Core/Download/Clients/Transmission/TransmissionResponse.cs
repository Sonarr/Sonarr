using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public class TransmissionResponse
    {
        public string Result { get; set; }
        public Dictionary<string, object> Arguments { get; set; }
    }
}
