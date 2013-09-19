using System;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class JsonRequest
    {
        public String Method { get; set; }
        public object[] Params { get; set; }
    }
}
