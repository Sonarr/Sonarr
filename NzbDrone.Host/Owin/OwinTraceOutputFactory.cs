using System.IO;
using Microsoft.Owin.Hosting.Tracing;

namespace NzbDrone.Host.Owin
{
    public class OwinTraceOutputFactory : ITraceOutputFactory
    {

        public TextWriter Create(string outputFile)
        {
            return new NlogTextWriter();
        }
    }
}