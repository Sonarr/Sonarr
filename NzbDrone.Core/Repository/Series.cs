using System;
using System.Collections.Generic;
using NzbDrone.Core.Repository.Quality;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Series
    {
        [SubSonicPrimaryKey(false)]
        public virtual int SeriesId { get; set; }

        public string Title { get; set; }

        public string CleanTitle { get; set; }

        [SubSonicNullString]
        public string Status { get; set; }

        [SubSonicLongString]
        public string Overview { get; set; }

        public DayOfWeek? AirsDayOfWeek { get; set; }

        public String AirTimes { get; set; }

        public string Language { get; set; }

        public string Path { get; set; }

        public bool Monitored { get; set; }

        public int QualityProfileId { get; set; }

        public bool SeasonFolder { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual QualityProfile QualityProfile { get; private set; }

        [SubSonicToManyRelation]
        public virtual List<Season> Seasons { get; private set; }

        [SubSonicToManyRelation]
        public virtual List<Episode> Episodes { get; private set; }

        [SubSonicToManyRelation]
        public virtual List<EpisodeFile> EpisodeFiles { get; private set; }
    }
}