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

        public DayOfWeek? LastInfoSync { get; set; }

        public DayOfWeek? LastDiskSync { get; set; }

        [SubSonicToManyRelation]
        public virtual IList<Episode> Episodes { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Series Series { get; set; }
    }
}