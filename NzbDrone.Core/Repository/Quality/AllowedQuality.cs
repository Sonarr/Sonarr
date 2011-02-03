using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository.Quality
{
    public class AllowedQuality
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public int Order { get; set; }
        public bool MarkComplete { get; set; }
        public QualityTypes Quality { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual QualityProfile QualityProfile { get; private set; }
    }
}
