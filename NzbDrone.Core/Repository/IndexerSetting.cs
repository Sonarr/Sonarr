using System;
using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("IndexerSettings")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class IndexerSetting
    {
        public int Id { get; set; }

        public Boolean Enable { get; set; }

        public String IndexProviderType { get; set; }

        public String Name { get; set; }
    }
}