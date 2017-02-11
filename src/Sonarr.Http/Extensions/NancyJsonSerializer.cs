using System.Collections.Generic;
using System.IO;
using Nancy;
using NzbDrone.Common.Serializer;

namespace Sonarr.Http.Extensions
{
    public class NancyJsonSerializer : ISerializer
    {
        public bool CanSerialize(string contentType)
        {
            return true;
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            Json.Serialize(model, outputStream);
        }

        public IEnumerable<string> Extensions { get; private set; }
    }
}