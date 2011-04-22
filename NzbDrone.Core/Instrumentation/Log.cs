using System;
using SubSonic.SqlGeneration.Schema;

namespace NzbDrone.Core.Instrumentation
{
    public class Log
    {
        [SubSonicPrimaryKey]
        public int LogId { get; protected set; }

        [SubSonicLongString]
        public string Message { get; set; }

        public DateTime Time { get; set; }

        public string Logger { get; set; }

        public string Method { get; set; }

        [SubSonicNullString]
        [SubSonicLongString]
        public string Exception { get; set; }

        [SubSonicNullString]
        public string ExceptionType { get; set; }

        public String Level { get; set; }

    }
}