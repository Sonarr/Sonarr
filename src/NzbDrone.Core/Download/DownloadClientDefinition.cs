using System;
using System.Collections.Generic;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public class DownloadClientDefinition : ProviderDefinition
    {
        public DownloadClientDefinition()
        {
            Tags = new List<Int32>();
        }

        public DownloadProtocol Protocol { get; set; }
        public List<Int32> Tags { get; set; }
    }
}
