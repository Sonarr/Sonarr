using System;
using System.ComponentModel.DataAnnotations;
using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("NewznabDefinitions")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class NewznabDefinition
    {
        public int Id { get; set; }

        public Boolean Enable { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String Name { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String Url { get; set; }

        public String ApiKey { get; set; }

        public bool BuiltIn { get; set; }
    }
}