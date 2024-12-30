namespace Workarr.Notifications.Plex.PlexTv
{
    public class PlexTvPinUrlResponse
    {
        public string Url { get; set; }
        public string Method => "POST";
        public Dictionary<string, string> Headers { get; set; }
    }
}
