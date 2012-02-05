using System.Linq;
using PetaPoco;

namespace NzbDrone.Services.Service.Repository.Reporting
{
    [TableName("ParseErrorReports")]
    public class ParseErrorRow : ReportRowBase
    {
        public string Title { get; set; }
    }
}
