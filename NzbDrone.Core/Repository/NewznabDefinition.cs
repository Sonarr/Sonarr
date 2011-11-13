using System;
using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("NewznabDefinitions")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class NewznabDefinition
    {
        public int Id { get; set; }

        public Boolean Enable { get; set; }

        public String Name { get; set; }

        public String Url { get; set; }

        public String ApiKey { get; set; }
    }
}