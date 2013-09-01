using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Api.RootFolders
{
    public class RootFolderResource : RestResource
    {
        public String Path { get; set; }
        public Int64? FreeSpace { get; set; }

        public List<UnmappedFolder> UnmappedFolders { get; set; }
    }
}