using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Config
{
    public class IndexerConfigResource : RestResource
    {
        public Int32 Retention { get; set; }
        public Int32 RssSyncInterval { get; set; }
        public String ReleaseRestrictions { get; set; }
    }
}
