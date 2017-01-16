using System.IO;
using System.Text;
using NLog;

namespace NzbDrone.Host.Owin
{
    public class NlogTextWriter : TextWriter
    {
        private readonly Logger _logger;

        public NlogTextWriter(Logger logger)
        {
            _logger = logger;
        }

        public override Encoding Encoding => Encoding.Default;

        public override void Write(char[] buffer, int index, int count)
        {
            Write(buffer);
        }
        public override void Write(char[] buffer)
        {
            Write(new string(buffer));
        }

        public override void Write(string value)
        {
            _logger.Log(GetLogLevel(value), value);
        }

        public override void Write(char value)
        {
            _logger.Trace(value);
        }

        private LogLevel GetLogLevel(string value)
        {
            var lower = value.ToLowerInvariant();

            if (!lower.Contains("error"))
            {
                return LogLevel.Trace;
            }

            if (lower.Contains("sqlite"))
            {
                return LogLevel.Trace;
            }

            if (lower.Contains("\"errors\":null"))
            {
                return LogLevel.Trace;
            }
            
            if (lower.Contains("signalr"))
            {
                if (lower.Contains("an operation was attempted on a nonexistent network connection"))
                {
                    return LogLevel.Trace;
                }

                if (lower.Contains("the network connection was aborted by the local system"))
                {
                    return LogLevel.Trace;
                }

                if (lower.Contains("the socket has been shut down"))
                {
                    return LogLevel.Trace;
                }
            }

            return LogLevel.Error;
        }
    }
}