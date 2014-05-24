using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetResponse<T>
    {
        public String Version { get; set; }

        public T Result { get; set; }

    }
}
