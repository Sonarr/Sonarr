using System;
using Newtonsoft.Json;
using NzbDrone.Api.EpisodeFiles;
using NzbDrone.Api.REST;
using NzbDrone.Api.Series;

namespace NzbDrone.Api.Episodes
{
    public class EpisodeResource : RestResource
    {
        public int SeriesId { get; set; }
        public int EpisodeFileId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
        public DateTime? AirDateUtc { get; set; }
        public string Overview { get; set; }
        public EpisodeFileResource EpisodeFile { get; set; }

        public bool HasFile { get; set; }
        public bool Monitored { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }
        public int? SceneAbsoluteEpisodeNumber { get; set; }
        public int? SceneEpisodeNumber { get; set; }
        public int? SceneSeasonNumber { get; set; }
        public bool UnverifiedSceneNumbering { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? GrabDate { get; set; }
        public string SeriesTitle { get; set; }
        public SeriesResource Series { get; set; }

        //Hiding this so people don't think its usable (only used to set the initial state)
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Grabbed { get; set; }
    }
}
