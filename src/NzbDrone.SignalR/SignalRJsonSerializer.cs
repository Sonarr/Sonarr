using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;

namespace NzbDrone.SignalR
{
    public static class SignalRJsonSerializer
    {
        private static JsonSerializer _serializer;
        private static JsonSerializerSettings _serializerSettings;

        public static void Register()
        {
            _serializerSettings = Json.GetSerializerSettings();
            _serializerSettings.ContractResolver = new SignalRContractResolver();

            _serializer = JsonSerializer.Create(_serializerSettings);

            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => _serializer);
        }
    }
}
