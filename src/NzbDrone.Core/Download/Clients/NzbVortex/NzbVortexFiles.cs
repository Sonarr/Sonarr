using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.NzbVortex
{
    public class NzbVortexFiles
    {
        public List<NzbVortexFile> Files { get; set; }
    }
}
