using System.IO;
using Microsoft.Owin.Hosting.Tracing;
using NLog;

namespace NzbDrone.Host.Owin
{
    public class OwinTraceOutputFactory : ITraceOutputFactory
    {
        private readonly Logger logger = LogManager.GetLogger("Owin");

        public TextWriter Create(string outputFile)
        {
            return new NlogTextWriter(logger);
        }
    }
}