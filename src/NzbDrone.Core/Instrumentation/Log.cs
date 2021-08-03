using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Instrumentation
{
    public class Log : ModelBase
    {
        public string Message { get; set; }

        public DateTime Time { get; set; }

        public string Logger { get; set; }

        public string Exception { get; set; }

        public string ExceptionType { get; set; }

        public string Level { get; set; }
    }
}
