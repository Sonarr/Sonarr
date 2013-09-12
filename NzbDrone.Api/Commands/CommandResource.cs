using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.Messaging.Tracking;

namespace NzbDrone.Api.Commands
{
    public class CommandResource : RestResource
    {
        public String Name { get; set; }
        public String Message { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime StateChangeTime { get; set; }
        public Boolean SendUpdatesToClient { get; set; }
        public CommandStatus State { get; set; }
    }
}