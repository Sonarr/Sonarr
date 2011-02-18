using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Repository.Quality;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class History
    {
        public int HistoryId { get; set; }
        public virtual int EpisodeId { get; set; }
        public virtual string IndexerName { get; set; }
        public QualityTypes Quality { get; set; }
        public DateTime Date { get; set; }
        public bool IsProper { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Episode Episode { get; private set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Indexer Indexer { get; private set; }
    }
}
