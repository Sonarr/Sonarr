using System.Text.Json.Serialization;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Xbmc
{
    public class KodiEpisodeGuide
    {
        [JsonPropertyName("tvdb")]
        public string Tvdb { get; set; }

        [JsonPropertyName("tvmaze")]
        public string TvMaze { get; set; }

        [JsonPropertyName("tvrage")]
        public string TvRage { get; set; }

        [JsonPropertyName("tmdb")]
        public string Tmdb { get; set; }

        [JsonPropertyName("imdb")]
        public string Imdb { get; set; }

        public KodiEpisodeGuide()
        {
        }

        public KodiEpisodeGuide(Series series)
        {
            Tvdb = series.TvdbId.ToString();
            TvMaze = series.TvMazeId > 0 ? series.TvMazeId.ToString() : null;
            TvRage = series.TvRageId > 0 ? series.TvMazeId.ToString() : null;
            Tmdb = series.TmdbId > 0 ? series.TmdbId.ToString() : null;
            Imdb = series.ImdbId;
        }
    }
}
