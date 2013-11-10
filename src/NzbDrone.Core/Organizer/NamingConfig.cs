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
                        MultiEpisodeStyle = 0,
                        StandardEpisodeFormat = "",
                        DailyEpisodeFormat = ""
                    };
            }
        }

        public bool RenameEpisodes { get; set; }
        public int MultiEpisodeStyle { get; set; }
        public string StandardEpisodeFormat { get; set; }
        public string DailyEpisodeFormat { get; set; }
    }
}