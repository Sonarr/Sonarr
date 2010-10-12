using NzbDrone.Core.Entities.Quality;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Entities
{
    public class MediaFile
    {
        [SubSonicPrimaryKey]
        public virtual int FileId { get; set; }
        public string Path { get; set; }
        public QualityTypes Quality { get; set; }
        public long Size { get; set; }
        public bool Proper { get; set; }
    }
}
