using System.Collections.Generic;
using PetaPoco;

namespace NzbDrone.Core.Repository
{
    public interface IRootDir
    {
        int Id { get; set; }
        string Path { get; set; }

        [ResultColumn]
        ulong FreeSpace { get; set; }

        [Ignore]
        List<string> UnmappedFolders { get; set; }
    }

    [TableName("RootDirs")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class RootDir : IRootDir
    {
        public virtual int Id { get; set; }

        public string Path { get; set; }

        [ResultColumn]
        public ulong FreeSpace { get; set; }

        [Ignore]
        public List<string> UnmappedFolders { get; set; }
    }
}