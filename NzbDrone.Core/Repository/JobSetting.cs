using System;
using PetaPoco;

namespace NzbDrone.Core.Repository
{
    [TableName("JobSettings")]
    [PrimaryKey("Id", autoIncrement = true)]
    public class JobSetting
    {
        public Int32 Id { get; set; }

        public Boolean Enable { get; set; }

        public String TypeName { get; set; }

        public String Name { get; set; }

        public Int32 Interval { get; set; }

        public DateTime LastExecution { get; set; }

        public Boolean Success { get; set; }
    }
}