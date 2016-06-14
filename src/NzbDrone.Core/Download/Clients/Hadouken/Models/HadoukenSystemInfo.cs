using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Hadouken.Models
{
    public sealed class HadoukenSystemInfo
    {
        public string Commitish { get; set; }
        public string Branch { get; set; }
        public Dictionary<string, string> Versions { get; set; }
    }
}
