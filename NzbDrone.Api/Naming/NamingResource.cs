using NzbDrone.Api.Config;

namespace NzbDrone.Api.Naming
{
    public class NamingResource : NamingConfigResource
    {
        public string SingleEpisodeExample { get; set; }
        public string MultiEpisodeExample { get; set; }
    }
}