using System.Collections.Generic;
using Sonarr.Http.REST;
using NzbDrone.Core.ThingiProvider;
using Sonarr.Http.ClientSchema;

namespace NzbDrone.Api
{
    public class ProviderResource : RestResource
    {
        public string Name { get; set; }
        public List<Field> Fields { get; set; }
        public string ImplementationName { get; set; }
        public string Implementation { get; set; }
        public string ConfigContract { get; set; }
        public string InfoLink { get; set; }
        public ProviderMessage Message { get; set; }

        public List<ProviderResource> Presets { get; set; }
    }
}