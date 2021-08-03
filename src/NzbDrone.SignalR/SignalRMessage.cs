using Newtonsoft.Json;
using NzbDrone.Core.Datastore.Events;

namespace NzbDrone.SignalR
{
    public class SignalRMessage
    {
        public object Body { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public ModelAction Action { get; set; }
    }
}
