using System.IO;
using System.Text;
using NLog;

namespace NzbDrone.Core.Instrumentation
{
    public class NlogWriter : TextWriter
    {
        private static readonly Logger Logger = LogManager.GetLogger("NzbDrone.SubSonic");

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }


        public override void Write(char[] buffer, int index, int count)
        {
            Write(new string(buffer, index, count));
        }

        public override void Write(string value)
        {
            DbAction(value);
        }

        private static void DbAction(string value)
        {
            Logger.Trace(value);
        }
    }
}