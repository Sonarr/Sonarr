using System;
using PetaPoco;

namespace NzbDrone.Core.Instrumentation
{
    [TableName("Logs")]
    [PrimaryKey("LogId", autoIncrement = true)]
    public class Log
    {

        public Int64 LogId { get; protected set; }

        public string Message { get; set; }

        public DateTime Time { get; set; }

        public string Logger { get; set; }

        public string Method { get; set; }

        public string Exception { get; set; }

        public string ExceptionType { get; set; }

        public String Level { get; set; }

    }
}