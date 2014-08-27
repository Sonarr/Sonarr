using System;
using Newtonsoft.Json;
using NzbDrone.Api.EpisodeFiles;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Episodes
{
    public class EpisodeResource : RestResource
    {
        public Int32 SeriesId { get; set; }
        public Int32 EpisodeFileId { get; set; }
        public Int32 SeasonNumber { get; set; }
        public Int32 EpisodeNumber { get; set; }
        public String Title { get; set; }
        public String AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public String Overview { get; set; }
        public EpisodeFileResource EpisodeFile { get; set; }

        public Boolean HasFile { get; set; }
        public Boolean Monitored { get; set; }
        public Nullable<Int32> SceneAbsoluteEpisodeNumber { get; set; }
        public Int32 SceneEpisodeNumber { get; set; }
        public Int32 SceneSeasonNumber { get; set; }
        public Int32 TvDbEpisodeId { get; set; }
        public Int32? AbsoluteEpisodeNumber { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? GrabDate { get; set; }
        public String SeriesTitle { get; set; }

        //I'd like to replace this with SeriesResource, but LoadSubType would need to be reworked to support that
        public Core.Tv.Series Series { get; set; }

        //Hiding this so people don't think its usable (only used to set the initial state)
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Boolean Grabbed { get; set; }
    }
}
