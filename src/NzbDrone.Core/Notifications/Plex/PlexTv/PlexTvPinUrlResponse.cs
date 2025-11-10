using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Plex.PlexTv
{
    public class PlexTvPinUrlResponse
    {
        public bool Poll { get; set; }
        public string Url { get; set; }
        public string Method => "POST";
        public Dictionary<string, string> Headers { get; set; }
    }
}
