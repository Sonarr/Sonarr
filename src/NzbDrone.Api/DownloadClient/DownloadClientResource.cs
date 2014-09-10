using System;
using System.Collections.Generic;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Api.DownloadClient
{
    public class DownloadClientResource : ProviderResource
    {
        public Boolean Enable { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public List<Int32> Tags { get; set; }
    }
}