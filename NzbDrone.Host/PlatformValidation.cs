using System;
using System.Diagnostics;
using System.Reflection;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Host
{
    public static class PlatformValidation
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static bool IsValidate(IUserAlert userAlert)
        {
            if (OsInfo.IsMono)
            {
                return true;
            }

            if (!IsAssemblyAvailable("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"))
            {
                userAlert.Alert("It looks like you don't have full version of .NET Framework installed. You will now be directed the download page.");

                try
                {
                    Process.Start("http://www.microsoft.com/en-ca/download/details.aspx?id=30653");
                }
                catch (Exception e)
                {
                    userAlert.Alert("Oops. can't start default browser. Please visit http://www.microsoft.com/en-ca/download/details.aspx?id=30653 to download .NET Framework 4.5.");
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
                Logger.Warn("Couldn't load {0}", e.Message);
                return false;
            }

        }
    }
}