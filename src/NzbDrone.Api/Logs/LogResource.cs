using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Logs
{
    public class LogResource : RestResource
    {
        public DateTime Time { get; set; }
        public String Exception { get; set; }
        public String ExceptionType { get; set; }
        public String Level { get; set; }
        public String Logger { get; set; }
        public String Message { get; set; }
        public String Method { get; set; }
    }
}
