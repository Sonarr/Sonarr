using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Series
{
    public class SeriesResource : RestResource
    {
        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire Profile instead of ID and Name separately

        //View Only
        public String Title { get; set; }
        public List<AlternateTitleResource> AlternateTitles { get; set; }
        public String SortTitle { get; set; }

        public Int32 SeasonCount
        {
            get
            {
                if (Seasons == null) return 0;

                return Seasons.Where(s => s.SeasonNumber > 0).Count();
            }
        }

        public Int32 EpisodeCount { get; set; }
        public Int32 EpisodeFileCount { get; set; }
        public Int64 SizeOnDisk { get; set; }
        public SeriesStatusType Status { get; set; }
        public String ProfileName { get; set; }
        public String Overview { get; set; }
        public DateTime? NextAiring { get; set; }
        public DateTime? PreviousAiring { get; set; }
        public String Network { get; set; }
        public String AirTime { get; set; }
        public List<MediaCover> Images { get; set; }

        public String RemotePoster { get; set; }
        public List<SeasonResource> Seasons { get; set; }
        public Int32 Year { get; set; }

        //View & Edit
        public String Path { get; set; }
        public Int32 ProfileId { get; set; }

        //Editing Only
        public Boolean SeasonFolder { get; set; }
        public Boolean Monitored { get; set; }

        public Boolean UseSceneNumbering { get; set; }
        public Int32 Runtime { get; set; }
        public Int32 TvdbId { get; set; }
        public Int32 TvRageId { get; set; }
        public DateTime? FirstAired { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public SeriesTypes SeriesType { get; set; }
        public String CleanTitle { get; set; }
        public String ImdbId { get; set; }
        public String TitleSlug { get; set; }
        public String RootFolderPath { get; set; }
        public String Certification { get; set; }
        public List<String> Genres { get; set; }
        public HashSet<Int32> Tags { get; set; }

        //Used to support legacy consumers
        public Int32 QualityProfileId
        {
            get
            {
                return ProfileId;
            }
            set
            {
                if (value > 0 && ProfileId == 0)
                {
                    ProfileId = value;
                }
            }
        }
    }
}
