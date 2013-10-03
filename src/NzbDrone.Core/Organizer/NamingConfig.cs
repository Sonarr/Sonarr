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
                        Separator = " - ",
                        NumberStyle = 0,
                        IncludeSeriesTitle = true,
                        MultiEpisodeStyle = 0,
                        IncludeEpisodeTitle = true,
                        IncludeQuality = true,
                        ReplaceSpaces = false
                    };
            }
        }

        public bool RenameEpisodes { get; set; }

        public string Separator { get; set; }

        public int NumberStyle { get; set; }

        public bool IncludeSeriesTitle { get; set; }

        public bool IncludeEpisodeTitle { get; set; }

        public bool IncludeQuality { get; set; }

        public int MultiEpisodeStyle { get; set; }

        public bool ReplaceSpaces { get; set; }
    }
}