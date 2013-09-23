using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class ActivePlayersEdenResult
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public List<ActivePlayer> Result { get; set; }
    }
}