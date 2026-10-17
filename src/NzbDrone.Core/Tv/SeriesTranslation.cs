using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Tv
{
    public class SeriesTranslation : ModelBase
    {
        public int SeriesId { get; set; }
        public Language Language { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string Overview { get; set; }
    }
}
