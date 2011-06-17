using PetaPoco;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Repository
{
    [PrimaryKey("Key", autoIncrement = false)]
    public class Config
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}