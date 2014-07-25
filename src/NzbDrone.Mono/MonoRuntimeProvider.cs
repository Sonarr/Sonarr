using System;
using System.Reflection;
using NLog;
using NzbDrone.Common.Processes;

namespace NzbDrone.Mono
{
    public class MonoRuntimeProvider : IRuntimeProvider
    {
        private readonly Logger _logger;

        public MonoRuntimeProvider(Logger logger)
        {
            _logger = logger;
        }

        public String GetVersion()
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
