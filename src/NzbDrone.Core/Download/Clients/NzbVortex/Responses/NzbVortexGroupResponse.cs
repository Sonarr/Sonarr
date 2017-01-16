using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.NzbVortex.Responses
{
    public class NzbVortexGroupResponse : NzbVortexResponseBase
    {
        public List<NzbVortexGroup> Groups { get; set; }
    }
}
