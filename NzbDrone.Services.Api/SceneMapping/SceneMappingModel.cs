using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;


namespace NzbDrone.Services.Api.SceneMapping
{
    public class SceneMappingModel
    {
        public const string CollectionName = "SceneMappings";

        [BsonId]
        public String Id { get; set; }

        [BsonElement("ct")]
        public string CleanTitle { get; set; }

        [BsonElement("si")]
        [JsonProperty(PropertyName = "id")]
        public int SeriesId { get; set; }

        [BsonElement("sn")]
        [JsonProperty(PropertyName = "Title")]
        public string SceneName { get; set; }

        [BsonElement("s")]
        [JsonProperty(PropertyName = "Season")]
        public int SeasonNumber { get; set; }
    }
}