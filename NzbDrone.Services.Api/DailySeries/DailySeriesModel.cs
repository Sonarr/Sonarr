using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;


namespace NzbDrone.Services.Api.DailySeries
{
    public class DailySeriesModel
    {
        public const string CollectionName = "DailySeries";

        [BsonId]
        public Int32 Id { get; set; }

        [BsonElement("t")]
        public String Title { get; set; }

        [BsonElement("p")]
        public Boolean Public { get; set; }
    }
}