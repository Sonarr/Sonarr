using System;
using System.Collections.Generic;
using NzbDrone.Core.Model;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class Episode
    {
        [SubSonicPrimaryKey]
        public virtual int EpisodeId { get; set; }

        public int? TvDbEpisodeId { get; set; }

        public virtual int SeriesId { get; set; }
        public virtual int EpisodeFileId { get; set; }
        public virtual int SeasonId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public DateTime AirDate { get; set; }

        [SubSonicLongString]
        public string Overview { get; set; }

        public Boolean Ignored { get; set; }


        /// <summary>
        /// Gets or sets the grab date.
        /// </summary>
        /// <remarks>
        /// Used to specify when the episode was grapped.
        /// this filed is used by status as an expirable "Grabbed" status.
        /// </remarks>
        public DateTime? GrabDate { get; set; }

        [SubSonicIgnore]
        public EpisodeStatusType Status
        {
            get
            {
                if (Ignored || (Season != null && !Season.Monitored)) return EpisodeStatusType.Ignored;

                if (GrabDate != null && GrabDate.Value.AddDays(1) >= DateTime.Now)
                {
                    return EpisodeStatusType.Downloading;
                }

                if (EpisodeFileId != 0) return EpisodeStatusType.Ready;

                if (DateTime.Now.Date >= AirDate.Date)
                {
                    return EpisodeStatusType.Missing;
                }

                return EpisodeStatusType.NotAired;
            }
        }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Season Season { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual Series Series { get; set; }

        [SubSonicToOneRelation(ThisClassContainsJoinKey = true)]
        public virtual EpisodeFile EpisodeFile { get; set; }

        [SubSonicToManyRelation]
        public virtual IList<History> Histories { get; protected set; }

    }
}