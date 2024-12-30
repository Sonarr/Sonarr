namespace Workarr.Notifications.Xbmc.Model
{
    public class TvShowResult
    {
        public Dictionary<string, int> Limits { get; set; }
        public List<TvShow> TvShows;

        public TvShowResult()
        {
            TvShows = new List<TvShow>();
        }
    }
}
