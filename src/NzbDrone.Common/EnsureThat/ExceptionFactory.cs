using System;
using NLog;

namespace NzbDrone.Common.EnsureThat
{
    internal static class ExceptionFactory
    {
        private static readonly Logger Logger = LogManager.GetLogger("ArgumentValidator");

        internal static ArgumentException CreateForParamValidation(string paramName, string message)
        {
            Logger.Warn(message);
            return new ArgumentException(message, paramName);
        }

        internal static ArgumentNullException CreateForParamNullValidation(string paramName, string message)
        {
            Logger.Warn(message);
            return new ArgumentNullException(paramName, message);
        }
    }
}
