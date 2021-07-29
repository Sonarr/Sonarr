using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class ActivePlayersResult
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public List<ActivePlayer> Result { get; set; }
    }
}
