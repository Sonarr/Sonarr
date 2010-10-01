using System;
using NzbDrone.Core.Repository.Quality;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Episode
    {
        [SubSonicPrimaryKey(false)]
        public long EpisodeId { get; set; }

        public long SeriesId { get; set; }
        public string Title { get; set; }
        public long SeasonId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public DateTime AirDate { get; set; }
        public QualityTypes Quality { get; set; }
        public bool Proper { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Season Season { get; private set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Series Series { get; private set; }
    }
}