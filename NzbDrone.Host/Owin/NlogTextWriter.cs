using System.IO;
using System.Text;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Host.Owin
{
    public class NlogTextWriter : TextWriter
    {
        private readonly Logger logger =  NzbDroneLogger.GetLogger();


        public override Encoding Encoding
        {
            get
            {
                return Encoding.Default;
            }
        }

        public override void Write(char value)
        {
            logger.Trace(value);
        }

        public override void Write(char[] buffer)
        {
            logger.Trace(buffer);
        }

        public override void Write(string value)
        {
            logger.Trace(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            logger.Trace(buffer);
        }

    }
}