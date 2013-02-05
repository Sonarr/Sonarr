using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using PetaPoco;

namespace NzbDrone.Core.RootFolders
{

    [TableName("RootDirs")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class RootFolder : BaseRepositoryModel
    {
        public string Path { get; set; }

        [ResultColumn]
        public ulong FreeSpace { get; set; }

        [Ignore]
        public List<string> UnmappedFolders { get; set; }
    }
}