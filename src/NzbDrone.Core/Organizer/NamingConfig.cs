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
                        RenameEpisodes = true,
                        MultiEpisodeStyle = 0,
                        StandardEpisodeFormat = "{Series Title} - {season}x{0episode} - {Episode Title} {Quality Title}",
                        DailyEpisodeFormat = "{Series Title} - {Air Date} - {Episode Title} {Quality Title}"
                    };
            }
        }

        public bool RenameEpisodes { get; set; }
        public int MultiEpisodeStyle { get; set; }
        public string StandardEpisodeFormat { get; set; }
        public string DailyEpisodeFormat { get; set; }
    }
}