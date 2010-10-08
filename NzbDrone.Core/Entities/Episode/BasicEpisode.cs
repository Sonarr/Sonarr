using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Entities.Episode
{
    public class BasicEpisode
    {
        public virtual int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Series Series { get; private set; }
    }
}