using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.RemotePathMappings
{
    public class RemotePathMappingResource : RestResource
    {
        public String Host { get; set; }
        public String RemotePath { get; set; }
        public String LocalPath { get; set; }
    }
}
