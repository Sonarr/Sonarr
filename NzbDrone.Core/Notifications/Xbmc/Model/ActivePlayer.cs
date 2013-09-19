namespace NzbDrone.Core.Notifications.Xbmc.Model
{
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
