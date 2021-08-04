using System.Collections.Generic;
using System.IO;
using Nancy;
using Nancy.Responses.Negotiation;
using NzbDrone.Common.Serializer;

namespace Sonarr.Http.Extensions
{
    public class NancyJsonSerializer : ISerializer
    {
        public bool CanSerialize(MediaRange contentType)
        {
            return contentType == "application/json";
        }

        public void Serialize<TModel>(MediaRange contentType, TModel model, Stream outputStream)
        {
            Json.Serialize(model, outputStream);
        }

        public IEnumerable<string> Extensions { get; private set; }
    }
}
