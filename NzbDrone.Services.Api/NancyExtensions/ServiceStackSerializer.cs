using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Nancy;
using ServiceStack.Text;

namespace NzbDrone.Services.Api.NancyExtensions
{
    public class ServiceStackSerializer : ISerializer
    {
        public readonly static ServiceStackSerializer Instance = new ServiceStackSerializer();

        public bool CanSerialize(string contentType)
        {
            return true;
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            JsonSerializer.SerializeToStream(model, outputStream);
        }

        public IEnumerable<string> Extensions { get; private set; }
    }
}