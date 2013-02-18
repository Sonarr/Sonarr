using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using Sqo.Attributes;

namespace NzbDrone.Core.RootFolders
{
    public class UnmappedFolder
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}