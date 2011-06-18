using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("ExternalNotificationSettings")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class ExternalNotificationSetting
    {
        public int Id { get; set; }

        public bool Enabled { get; set; }

        public string NotifierName { get; set; }

        public string Name { get; set; }
    }
}
