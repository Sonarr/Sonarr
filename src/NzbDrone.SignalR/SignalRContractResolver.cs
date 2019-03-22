using System;
using Newtonsoft.Json.Serialization;

namespace NzbDrone.SignalR
{
    public class SignalRContractResolver : IContractResolver
    {
        private readonly IContractResolver _camelCaseContractResolver;
        private readonly IContractResolver _defaultContractSerializer;

        public SignalRContractResolver()
        {
            _defaultContractSerializer = new DefaultContractResolver();
            _camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        public JsonContract ResolveContract(Type type)
        {
            var fullName = type.FullName;
            if (fullName.StartsWith("NzbDrone") || fullName.StartsWith("Sonarr"))
            {
                return _camelCaseContractResolver.ResolveContract(type);
            }

            return _defaultContractSerializer.ResolveContract(type);
        }
    }
}
