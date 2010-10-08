using System.Collections.Generic;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Entities
{
    public class Season
    {
        [SubSonicPrimaryKey(false)]
        public virtual long SeasonId { get; set; }
        public long SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public bool Monitored { get; set; }
        public string Folder { get; set; }

        [SubSonicToManyRelation]
        public virtual List<Episode.BasicEpisode> Episodes { get; private set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Series Series { get; private set; }
    }
}