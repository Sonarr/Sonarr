using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Api.QualityProfiles;
using NzbDrone.Core.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Series
{
    public class SeriesResource
    {
        public Int32 Id { get; set; }

        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire QualityProfile instead of ID and Name separately

        //View Only
        public String Title { get; set; }
        public Int32 SeasonsCount { get; set; }
        public Int32 EpisodeCount { get; set; }
        public Int32 EpisodeFileCount { get; set; }
        public String Status { get; set; }
        public String AirsDayOfWeek { get; set; }
        public String QualityProfileName { get; set; }
        public String Overview { get; set; }
        public Int32 Episodes { get; set; }
        public Boolean HasBanner { get; set; }
        public DateTime NextAiring { get; set; }
        public String Details { get; set; }
        public String Network { get; set; }
        public String AirTime { get; set; }
        public String Language { get; set; }

        public Int32 SeasonCount { get; set; }
        public Int32 UtcOffset { get; set; }

        //View & Edit
        public String Path { get; set; }
        public Int32 QualityProfileId { get; set; }

        //Editing Only
        public Boolean SeasonFolder { get; set; }
        public Boolean Monitored { get; set; }
        public Int32 BacklogSetting { get; set; }
        public String CustomStartDate { get; set; }
    }
}
