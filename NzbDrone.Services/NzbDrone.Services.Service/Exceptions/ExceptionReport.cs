using System.Linq;

namespace NzbDrone.Services.Service.Exceptions
{
    public class ExceptionReport
    {
        public string ApplicationId { get; set; }
        public string AppVersion { get; set; }
        public string Uid { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string Stack { get; set; }
        public string Location { get; set; }
        public string Message { get; set; }
    }
}
