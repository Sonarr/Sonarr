using System;

namespace NzbDrone.Core.Organizer
{
    public class EpisodeFormat
    {
        public String Separator { get; set; }
        public String EpisodePattern { get; set; }
        public String EpisodeSeparator { get; set; }
        public String SeasonEpisodePattern { get; set; }
    }
}
