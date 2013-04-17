using System.Collections.Generic;
using System.IO;
using Nancy;
using NzbDrone.Common;

namespace NzbDrone.Api.Extensions
{
    public class NancyJsonSerializer : ISerializer
    {
        private readonly IJsonSerializer _jsonSerializer;

        public NancyJsonSerializer(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }


        public bool CanSerialize(string contentType)
        {
            return true;
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            _jsonSerializer.Serialize(model, outputStream);
        }

        public IEnumerable<string> Extensions { get; private set; }
    }
}