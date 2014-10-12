using System;
using System.Collections.Generic;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Datastore;


namespace NzbDrone.Core.RemotePathMappings
{
    public class RemotePathMapping : ModelBase
    {
        public String Host { get; set; }
        public String RemotePath { get; set; }
        public String LocalPath { get; set; }
    }
}