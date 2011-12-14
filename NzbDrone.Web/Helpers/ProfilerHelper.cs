using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NzbDrone.Common;

namespace NzbDrone.Web.Helpers
{
    public static class ProfilerHelper
    {
        public static bool Enabled()
        {
            var enviromentProvider = new EnviromentProvider();
            var configFileProvider = new ConfigFileProvider(enviromentProvider);

            return configFileProvider.EnableProfiler;
        }
    }
}