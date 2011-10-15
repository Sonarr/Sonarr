using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("RootDirs")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class RootDir
    {
        public virtual int Id { get; set; }

        public string Path { get; set; }

        [ResultColumn]
        public ulong FreeSpace { get; set; }
    }
}