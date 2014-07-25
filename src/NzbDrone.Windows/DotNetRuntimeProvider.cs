using System;
using NzbDrone.Common.Processes;

namespace NzbDrone.Windows
{
    public class DotNetRuntimeProvider : IRuntimeProvider
    {
        public String GetVersion()
        {
            return Environment.Version.ToString();
        }
    }
}
