using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Api.Series
{
    public class SeriesResource : RestResource
    {
        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire QualityProfile instead of ID and Name separately

        //View Only
        public String Title { get; set; }
        public Int32 SeasonsCount { get; set; }
        public Int32 EpisodeCount { get; set; }
        public Int32 EpisodeFileCount { get; set; }
        public SeriesStatusType Status { get; set; }
        public String QualityProfileName { get; set; }
        public String Overview { get; set; }
        public DateTime? NextAiring { get; set; }
        public String Network { get; set; }
        public String AirTime { get; set; }
        public Int32 UtcOffset { get; set; }
        public List<Core.MediaCover.MediaCover> Images { get; set; }

        public String Path { get; set; }

        //View & Edit
        public int RootFolderId { get; set; }
        public string FolderName { get; set; }
        public Int32 QualityProfileId { get; set; }

        //Editing Only
        public Boolean SeasonFolder { get; set; }
        public Boolean Monitored { get; set; }
        public BacklogSettingType BacklogSetting { get; set; }
        public DateTime? CustomStartDate { get; set; }

        public Boolean UseSceneNumbering { get; set; }
        public Int32 Id { get; set; }
        public Int32 Runtime { get; set; }
        public Int32 TvdbId { get; set; }
        public Int32 TvRageId { get; set; }
        public DateTime? FirstAired { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public SeriesTypes SeriesType { get; set; }
        public String CleanTitle { get; set; }
        public String ImdbId { get; set; }
        public String TitleSlug { get; set; }



    }
}
