using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Config
{
    public class IndexerConfigResource : RestResource
    {
        public int MinimumAge { get; set; }
        public int Retention { get; set; }
        public int RssSyncInterval { get; set; }
    }
}
