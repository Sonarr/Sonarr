using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Nancy;
using Nancy.Responses.Negotiation;
using NzbDrone.Common.Serializer;

namespace Sonarr.Http.Extensions
{
    public class NancyJsonSerializer : ISerializer
    {
        protected readonly JsonSerializerOptions _serializerSettings;

        public NancyJsonSerializer()
        {
            _serializerSettings = STJson.GetSerializerSettings();
        }

        public bool CanSerialize(MediaRange contentType)
        {
            return contentType == "application/json";
        }

        public void Serialize<TModel>(MediaRange contentType, TModel model, Stream outputStream)
        {
            STJson.Serialize(model, outputStream, _serializerSettings);
        }

        public IEnumerable<string> Extensions { get; private set; }
    }
}
