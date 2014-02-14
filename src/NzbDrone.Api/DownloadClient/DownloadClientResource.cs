using System;

namespace NzbDrone.Api.DownloadClient
{
    public class DownloadClientResource : ProviderResource
    {
        public Boolean Enable { get; set; }
        public Int32 Protocol { get; set; }
    }
}