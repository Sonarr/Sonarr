using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    public class RootDir
    {
        [SubSonicPrimaryKey]
        public virtual int Id { get; set; }

        public string Path { get; set; }
    }
}