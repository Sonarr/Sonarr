using System.Linq;
using System;
using MongoDB.Bson.Serialization.Attributes;

namespace NzbDrone.Services.Service.Exceptions
{
    public class ExceptionInstance
    {
        [BsonElement("ver")]
        public string AppVersion { get; set; }

        [BsonElement("uid")]
        public string UserId { get; set; }

        [BsonElement("xmsg")]
        public string ExceptionMessage { get; set; }

        [BsonElement("msg")]
        public string Message { get; set; }

        [BsonElement("time")]
        public DateTime Time { get; set; }
    }
}
