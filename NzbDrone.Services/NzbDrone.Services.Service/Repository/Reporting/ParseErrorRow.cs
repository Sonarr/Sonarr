using System.Linq;
using Services.PetaPoco;


namespace NzbDrone.Services.Service.Repository.Reporting
{
    [TableName("ParseErrorReports")]
    [PrimaryKey("Title", autoIncrement = false)]
    public class ParseErrorRow : ReportRowBase
    {
        public string Title { get; set; }
    }
}
