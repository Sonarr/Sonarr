using System.Linq;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace NzbDrone.Services.Service.Exceptions
{
    public class ExceptionInfo
    {
        public ExceptionInfo()
        {
            Instances = new ExceptionInstance[0];
        }

        [BsonId]
        public string Hash { get; set; }

        [BsonElement("xtype")]
        public string ExceptionType { get; set; }

        [BsonElement("stk")]
        public string Stack { get; set; }

        [BsonElement("loc")]
        public string Location { get; set; }

        [BsonElement("inst")]
        public IEnumerable<ExceptionInstance> Instances { get; set; }
    }
}
