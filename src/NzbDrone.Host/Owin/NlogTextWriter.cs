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

        public override Encoding Encoding
        {
            get
            {
                return Encoding.Default;
            }
        }

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
            if (value.ToLower().Contains("error"))
            {
                _logger.Error(value);
            }
            else
            {
                _logger.Trace(value);
            }
        }

        public override void Write(char value)
        {
            _logger.Trace(value);
        }
    }
}