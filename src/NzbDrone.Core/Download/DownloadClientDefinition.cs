using System;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public class DownloadClientDefinition : ProviderDefinition
    {
        public Boolean Enable { get; set; }
        public DownloadProtocol Protocol { get; set; }
    }
}
