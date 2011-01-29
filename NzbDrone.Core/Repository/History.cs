using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class History
    {
        public int HistoryId { get; set; }
        public virtual int EpisodeId { get; set; }
        public virtual string IndexerName { get; set; }
        public int Quality { get; set; }
        public DateTime Date { get; set; }
        public bool IsProper { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Episode Episode { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Indexer Indexer { get; set; }
    }
}
