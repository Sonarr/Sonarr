using System;
using System.IO;
using Microsoft.AspNet.SignalR.Json;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Api.SignalR
{
    [Singleton]
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

        public object Parse(string json, Type targetType)
        {
            if (targetType.FullName.StartsWith("NzbDrone"))
            {
                return Json.Deserialize(json, targetType);
            }

            return _signalRSerializer.Parse(json, targetType);
        }
    }
}