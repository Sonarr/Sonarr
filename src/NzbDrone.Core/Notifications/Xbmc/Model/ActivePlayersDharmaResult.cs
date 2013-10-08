using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class ActivePlayersDharmaResult
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public Dictionary<string, bool> Result { get; set; }
    }
}
