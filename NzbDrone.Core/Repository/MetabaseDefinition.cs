using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("MetabaseDefinitions")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class MetabaseDefinition
    {
        public int Id { get; set; }

        public bool Enable { get; set; }

        public string MetadataProviderType { get; set; }

        public string Name { get; set; }
    }
}