using Workarr.Datastore;

namespace Workarr.Instrumentation
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
