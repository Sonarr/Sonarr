using System;
using System.Collections.Generic;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Season
    {
        [SubSonicPrimaryKey(false)]
        public virtual int SeasonId { get; set; }
        public virtual int SeriesId { get; set; }
        public virtual int SeasonNumber { get; set; }
        public bool Monitored { get; set; }

        [SubSonicToManyRelation]
        public virtual List<Episode> Episodes { get; private set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Series Series { get; private set; }
    }
}