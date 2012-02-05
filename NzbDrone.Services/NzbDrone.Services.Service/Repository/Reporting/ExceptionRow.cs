using System.Linq;
using PetaPoco;

namespace NzbDrone.Services.Service.Repository.Reporting
{
    [TableName("ExceptionReports")]
    public class ExceptionRow : ReportRowBase
    {
        public string Type { get; set; }
        public string Logger { get; set; }
        public string LogMessage { get; set; }
        public string String { get; set; }
    }
}
