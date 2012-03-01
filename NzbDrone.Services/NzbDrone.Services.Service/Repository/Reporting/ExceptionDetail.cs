using System;
using System.Linq;
using Services.PetaPoco;

namespace NzbDrone.Services.Service.Repository.Reporting
{
    [TableName("Exceptions")]
    public class ExceptionDetail
    {
        public int Id { get; set; }
        public string Logger { get; set; }
        public string Type { get; set; }
        public string String { get; set; }
        public string Hash { get; set; }
        public string Version { get; set; }
    }
}