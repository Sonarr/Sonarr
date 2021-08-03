namespace NzbDrone.Core.Notifications.Trakt.Resource
{
    public class TraktShowIdsResource
    {
        public int Trakt { get; set; }
        public string Slug { get; set; }
        public string Imdb { get; set; }
        public int Tvdb { get; set; }
    }
}
