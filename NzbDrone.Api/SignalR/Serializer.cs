using System;
using System.IO;
using Microsoft.AspNet.SignalR.Json;

namespace NzbDrone.Api.SignalR
{
    public class Serializer : IJsonSerializer
    {
        private readonly Common.IJsonSerializer _nzbDroneSerializer;
        private JsonNetSerializer _signalRSerializer;

        public Serializer(Common.IJsonSerializer nzbDroneSerializer)
        {
            _signalRSerializer = new JsonNetSerializer();
            _nzbDroneSerializer = nzbDroneSerializer;

        }

        public void Serialize(object value, TextWriter writer)
        {
            if (value.GetType().FullName.StartsWith("NzbDrone"))
            {
                _nzbDroneSerializer.Serialize(value, writer);
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
                return _nzbDroneSerializer.Deserialize(json, targetType);
            }
     
            return _signalRSerializer.Parse(json, targetType);
        }
    }
}