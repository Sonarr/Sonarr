using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Organizer
{
    public class NameSpecification : ModelBase
    {
        public static NameSpecification Default
        {
            get { return new NameSpecification(); }
        }

        public bool UseSceneName { get; set; }

        public string Separator { get; set; }

        public int NumberStyle { get; set; }

        public bool IncludeSeriesName { get; set; }

        public int MultiEpisodeStyle { get; set; }

        public bool IncludeEpisodeTitle { get; set; }

        public bool AppendQuality { get; set; }

        public bool ReplaceSpaces { get; set; }

        public string SeasonFolderFormat { get; set; }
    }
}