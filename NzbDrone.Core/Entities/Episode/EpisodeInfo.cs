using System;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Entities.Episode
{
    [SubSonicTableNameOverride("EpisodeInfo")]
    public class EpisodeInfo : BasicEpisode
    {
        [SubSonicPrimaryKey(false)]
        public virtual int EpisodeId { get; set; }
        public int SeasonId { get; set; }
        public string Title { get; set; }
        public DateTime AirDate { get; set; }
        [SubSonicLongString]
        public string Overview { get; set; }
        public string Language { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Season Season { get; set; }
    }
}
