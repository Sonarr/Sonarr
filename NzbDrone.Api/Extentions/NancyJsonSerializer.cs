using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using Newtonsoft.Json;

namespace NzbDrone.Api.Extentions
{
    public class NancyJsonSerializer : ISerializer
    {
        public readonly static NancyJsonSerializer Instance = new NancyJsonSerializer();

        public bool CanSerialize(string contentType)
        {
            return true;
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            var jsonTextWriter = new JsonTextWriter(new StreamWriter(outputStream));
            Serializer.Instance.Serialize(jsonTextWriter, model);
            jsonTextWriter.Flush();
        }

        public IEnumerable<string> Extensions { get; private set; }
    }
}