using NzbDrone.Api.REST;

namespace NzbDrone.Api.Commands
{
    public class CommandResource : RestResource
    {
        public string Command { get; set; }
    }
}