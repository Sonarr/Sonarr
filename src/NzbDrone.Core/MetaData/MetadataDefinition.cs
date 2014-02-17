using System;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Metadata
{
    public class MetadataDefinition : ProviderDefinition
    {
        public Boolean Enable { get; set; }
    }
}
