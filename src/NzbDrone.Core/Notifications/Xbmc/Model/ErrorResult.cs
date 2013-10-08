using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class ErrorResult
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public Dictionary<string, string> Error { get; set; }
    }
}
