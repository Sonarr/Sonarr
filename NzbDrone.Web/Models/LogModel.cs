using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class LogModel
    {
        public string Message { get; set; }
        public string Time { get; set; }
        public string Source { get; set; }
        public string Method { get; set; }
        public string Exception { get; set; }
        public string ExceptionType { get; set; }
        public string Level { get; set; }
        public string Details { get; set; }
    }
}