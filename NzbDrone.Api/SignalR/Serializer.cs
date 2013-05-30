using System;
using System.IO;
using Microsoft.AspNet.SignalR.Json;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Api.SignalR
{
    public class Serializer : IJsonSerializer
    {
        private readonly JsonNetSerializer _signalRSerializer = new JsonNetSerializer();

        public void Serialize(object value, TextWriter writer)
        {
            if (value.GetType().FullName.StartsWith("NzbDrone"))
            {
                Json.Serialize(value, writer);
            }
            else
            {
                _signalRSerializer.Serialize(value, writer);
            }
        }

        public object Parse(TextReader reader, Type targetType)
        {
            return Json.Deserialize(reader.ReadToEnd(), targetType);
        }
    }
}