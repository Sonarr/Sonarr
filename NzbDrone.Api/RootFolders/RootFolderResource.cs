using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.RootFolders
{
    public class RootFolderResource : RestResource
    {
        public String Path { get; set; }
        public Int64 FreeSpace { get; set; }
    }
}