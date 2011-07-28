using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("ExternalNotificationDefinitions")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class ExternalNotificationDefinition
    {
        public int Id { get; set; }

        public bool Enable { get; set; }

        public string ExternalNotificationProviderType { get; set; }

        public string Name { get; set; }
    }
}