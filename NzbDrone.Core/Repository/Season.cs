using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Season
    {
        [SubSonicPrimaryKey(false)]
        public long SeasonId { get; set; }
        public long SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        public bool Monitored { get; set; }
        public string Folder { get; set; }

        [SubSonicToManyRelation]
        public virtual List<Episode> Episodes { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Series Series { get; set; }
    }
}