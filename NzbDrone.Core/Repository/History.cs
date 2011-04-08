using System;
using NzbDrone.Core.Repository.Quality;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class History
    {
        [SubSonicPrimaryKey]
        public virtual int HistoryId { get; set; }
        public virtual int EpisodeId { get; set; }
        public virtual int IndexerId { get; set; }
        public string NzbTitle { get; set; }
        public QualityTypes Quality { get; set; }
        public DateTime Date { get; set; }
        public bool IsProper { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Episode Episode { get; private set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Indexer Indexer { get; private set; }
    }
}
