using System.Collections.Generic;
using System.ComponentModel;

namespace Exceptron.Driver
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ExceptionReport
    {
        public string AppId { get; set; }
        public string AppVersion { get; set; }
        public string Uid { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public List<Frame> StackTrace { get; set; }
        public string Location { get; set; }
        public string Enviroment { get; set; }
        public string Message { get; set; }

        public string DriverName { get; set; }
        public string DriverVersion { get; set; }
    }
}