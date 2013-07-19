using System.Collections.Generic;

namespace NzbDrone.Core.Model.Xbmc
{
    public class ActivePlayersEdenResult
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public List<ActivePlayer> Result { get; set; }
    }

    public class ActivePlayer
    {
        public int PlayerId { get; set; }
        public string Type { get; set; }

        public ActivePlayer(int playerId, string type)
        {
            PlayerId = playerId;
            Type = type;
        }
    }
}
