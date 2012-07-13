using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("MetadataDefinitions")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class MetadataDefinition
    {
        public int Id { get; set; }

        public bool Enable { get; set; }

        public string MetadataProviderType { get; set; }

        public string Name { get; set; }
    }
}