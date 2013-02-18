using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using Sqo.Attributes;

namespace NzbDrone.Core.RootFolders
{
    public class RootFolder : ModelBase
    {
        public string Path { get; set; }

        [Ignore]
        public ulong FreeSpace { get; set; }

        public List<UnmappedFolder> UnmappedFolders { get; set; }
    }
}