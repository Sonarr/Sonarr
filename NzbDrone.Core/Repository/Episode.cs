using System;
using System.Collections.Generic;
using NzbDrone.Core.Model;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Episode
    {
        [SubSonicPrimaryKey(false)]
        public virtual int EpisodeId { get; set; }

        public virtual int SeriesId { get; set; }
        public virtual int EpisodeFileId { get; set; }
        public virtual int SeasonId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public DateTime AirDate { get; set; }

        [SubSonicLongString]
        public string Overview { get; set; }

        public string Language { get; set; }
        public EpisodeStatusType Status { get; set; }

        public DayOfWeek? LastInfoSync { get; set; }

        public DayOfWeek? LastDiskSync { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Season Season { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Series Series { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual EpisodeFile EpisodeFile { get; set; }

        [SubSonicToManyRelation]
        public virtual List<History> Histories { get; private set; }
    }
}