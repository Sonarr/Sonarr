using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Entities
{
    [SubSonicTableNameOverride("Config")]
    public class Config
    {
        [SubSonicPrimaryKey]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}