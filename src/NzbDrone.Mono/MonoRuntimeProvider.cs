using System;
using System.Reflection;
using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Mono
{
    public class MonoRuntimeProvider : RuntimeInfoBase
    {
        private readonly Logger _logger;

        public MonoRuntimeProvider(Common.IServiceProvider serviceProvider, Logger logger)
            :base(serviceProvider, logger)
        {
            _logger = logger;
        }

        public override string RuntimeVersion
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

                return string.Empty;
            }
        }
    }
}
