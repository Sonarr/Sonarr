using System.Linq;
using Services.PetaPoco;

namespace NzbDrone.Services.Service.Repository.Reporting
{
    [TableName("Exceptions")]
    [PrimaryKey("Hash", autoIncrement = false)]
    public class ExceptionDetail
    {
        public string Hash { get; set; }
        public string Logger { get; set; }
        public string Type { get; set; }
        public string String { get; set; }
        public string Version { get; set; }
    }
}