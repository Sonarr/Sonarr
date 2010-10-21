using System;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Episode
    {
        [SubSonicPrimaryKey(false)]
        public virtual int EpisodeId { get; set; }
        public virtual int SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonId { get; set; }
        public string Title { get; set; }
        public DateTime AirDate { get; set; }
        public string Overview { get; set; }
        public string Language { get; set; }

        [SubSonicNullString]
        public string Path { get; set; }
        public long? Size { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Season Season { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Series Series { get; private set; }
    }
}
