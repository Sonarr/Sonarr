namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class TvShowResponse
    {
        public string Id { get; set; }
        public string JsonRpc { get; set; }
        public TvShowResult Result { get; set; }
    }
}
