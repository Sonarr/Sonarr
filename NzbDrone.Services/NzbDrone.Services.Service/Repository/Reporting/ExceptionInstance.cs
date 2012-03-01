using System;
using System.Linq;
using Services.PetaPoco;

namespace NzbDrone.Services.Service.Repository.Reporting
{
    [TableName("ExceptionInstances")]
    public class ExceptionInstance 
    {
        public long Id { get; set; }
        public string ExceptionHash { get; set; }
        public string LogMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsProduction { get; set; }
        public Guid UGuid { get; set; }
    }
}