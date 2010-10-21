using NzbDrone.Core.Repository.Quality;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Model
{
    public class EpisodeModel
    {
        public string SeriesTitle { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public QualityTypes Quality { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public bool Proper { get; set; }
    }
}