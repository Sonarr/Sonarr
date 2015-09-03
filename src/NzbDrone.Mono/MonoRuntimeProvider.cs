using System;
using System.Reflection;
using System.Text;
using System.Runtime.InteropService;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using Mono.Unix.Native;

namespace NzbDrone.Mono
{
    public class MonoRuntimeProvider : RuntimeInfoBase
    {
        private readonly Logger _logger;

        public MonoRuntimeProvider(Common.IServiceProvider serviceProvider, Logger logger)
            :base(serviceProvider, logger)
        {
            _logger = logger;
            unixSetProcessName("sonarr");
        }

        [DllImport ("libc")] //Linux
        private static extern int prctl (int option, byte [] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);

        public void unixSetProcessName (string name)
        {
            try {
                var success = 0 == prctl(15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes (name + "\0"), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                if (!success)
                {
                    var error = Stdlib.GetLastError();
                    throw new InvalidOperationException("prtctl call error: " + error);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Could not set process name, error: " + ex.Message, ex);
            }
        }

        public override String RuntimeVersion
        {
            get
            {
                try
                {
                    var type = Type.GetType("Mono.Runtime");

                    if (type != null)
                    {
                        var displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);

                        if (displayName != null)
                        {
                            return displayName.Invoke(null, null).ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to get mono version: " + ex.Message, ex);
                }

                return String.Empty;
            }
        }
    }
}
