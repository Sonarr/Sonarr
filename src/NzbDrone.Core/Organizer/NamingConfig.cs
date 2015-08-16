using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Organizer
{
    public class NamingConfig : ModelBase
    {
        public static NamingConfig Default
        {
            get
            {
                return new NamingConfig
                    {
                        RenameEpisodes = false,
                        RenameMovies = false,
                        MultiEpisodeStyle = 0,
                        StandardEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}",
                        DailyEpisodeFormat = "{Series Title} - {Air-Date} - {Episode Title} {Quality Full}",
                        AnimeEpisodeFormat = "{Series Title} - S{season:00}E{episode:00} - {Episode Title} {Quality Full}",
                        SeriesFolderFormat = "{Series Title}",
                        SeasonFolderFormat = "Season {season}",
                        StandardMovieFormat = "{Movie Title} - {Quality Full}",
                        MovieFolderFormat = "{Movie Title} ({Year})"
                    };
            }
        }

        public bool RenameEpisodes { get; set; }
        public int MultiEpisodeStyle { get; set; }
        public string StandardEpisodeFormat { get; set; }
        public string DailyEpisodeFormat { get; set; }
        public string AnimeEpisodeFormat { get; set; }
        public string SeriesFolderFormat { get; set; }
        public string SeasonFolderFormat { get; set; }
        public string StandardMovieFormat { get; set; }
        public string MovieFolderFormat { get; set; }
        public bool RenameMovies { get; set; }
    }
}