using System;
using NzbDrone.Core.Datastore;
using Sqo.Attributes;

namespace NzbDrone.Core.Instrumentation
{
    public class Log : ModelBase
    {
        [Text]
        public string Message { get; set; }

        public DateTime Time { get; set; }

        public string Logger { get; set; }

        public string Method { get; set; }

        [Text]
        public string Exception { get; set; }

        public string ExceptionType { get; set; }

        public String Level { get; set; }
    }
}