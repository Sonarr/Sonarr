using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;


namespace NzbDrone.Services.Api.SceneMapping
{
    public class SceneMappingJsonModel
    {
        public String MapId { get; set; }

        public string CleanTitle { get; set; }

        [JsonProperty(PropertyName = "Id")]
        public int SeriesId { get; set; }

        [JsonProperty(PropertyName = "Title")]
        public string SceneName { get; set; }

        [JsonProperty(PropertyName = "Season")]
        public int SeasonNumber { get; set; }

        public Boolean Public { get; set; }
    }
}