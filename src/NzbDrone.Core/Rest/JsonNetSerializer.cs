using NzbDrone.Common.Serializer;
using RestSharp.Serializers;

namespace NzbDrone.Core.Rest
{
    public class JsonNetSerializer : ISerializer
    {
        public JsonNetSerializer()
        {
                ContentType = "application/json";
        }

        public string Serialize(object obj)
        {
            return obj.ToJson();
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public string ContentType { get; set; }
    }
}
