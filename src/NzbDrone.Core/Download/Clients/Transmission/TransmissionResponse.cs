using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public class TransmissionResponse
    {
        public String Result { get; set; }
        public Dictionary<String, Object> Arguments { get; set; }
    }
}
