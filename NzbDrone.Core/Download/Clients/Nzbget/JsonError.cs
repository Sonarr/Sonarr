using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class JsonError
    {
        public String Version { get; set; }
        public ErrorModel Error { get; set; }
    }
}
