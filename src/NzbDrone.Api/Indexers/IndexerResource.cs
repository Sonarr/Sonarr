using System;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Api.Indexers
{
    public class IndexerResource : ProviderResource
    {
        public Boolean Enable { get; set; }
        public DownloadProtocol Protocol { get; set; }
    }
}