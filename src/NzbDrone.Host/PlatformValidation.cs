using System;
using System.Diagnostics;
using System.Reflection;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Host
{
    public static class PlatformValidation
    {
        private const string DOWNLOAD_LINK = "http://www.microsoft.com/en-us/download/details.aspx?id=42643";
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(PlatformValidation));

        public static bool IsValidate(IUserAlert userAlert)
        {
            if (OsInfo.IsNotWindows)
            {
                return true;
            }

            if (!IsAssemblyAvailable("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"))
            {
                userAlert.Alert("It looks like you don't have the correct version of .NET Framework installed. You will now be directed the download page.");

                try
                {
                    Process.Start(DOWNLOAD_LINK);
                }
                catch (Exception)
                {
                    userAlert.Alert("Oops. Couldn't start your browser. Please visit http://www.microsoft.com/net to download the latest version of .NET Framework");
                }

                return false;
            }

            return true;
        }

        private static bool IsAssemblyAvailable(string assemblyString)
        {
            try
            {
                Assembly.Load(assemblyString);
                return true;
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Couldn't load {0}", assemblyString);
                return false;
            }
        }
    }
}
