using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class EnqueueResponse
    {
        public String Version { get; set; }
        public Boolean Result { get; set; }
    }
}
