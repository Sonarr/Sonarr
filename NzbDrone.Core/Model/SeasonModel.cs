using NzbDrone.Core.Repository.Quality;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Model
{
    public class SeasonModel
    {
        public string SeriesTitle { get; set; }
        public int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public QualityTypes Quality { get; set; }
        public long Size { get; set; }
        public bool Proper { get; set; }
    }
}