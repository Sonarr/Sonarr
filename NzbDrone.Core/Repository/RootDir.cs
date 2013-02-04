using System.Collections.Generic;
using NzbDrone.Core.RootFolders;
using PetaPoco;

namespace NzbDrone.Core.Repository
{

    [TableName("RootDirs")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class RootDir : BaseModel
    {
        public string Path { get; set; }

        [ResultColumn]
        public ulong FreeSpace { get; set; }

        [Ignore]
        public List<string> UnmappedFolders { get; set; }
    }
}