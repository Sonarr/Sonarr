using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using Sqo.Attributes;

namespace NzbDrone.Core.RootFolders
{
    public class RootFolder : BaseRepositoryModel
    {
        public string Path { get; set; }

        [Ignore]
        public ulong FreeSpace { get; set; }

        public List<string> UnmappedFolders { get; set; }
    }
}