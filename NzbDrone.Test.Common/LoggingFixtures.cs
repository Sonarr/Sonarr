using NLog;
using NLog.Config;
using NUnit.Framework;
using NzbDrone.Common;

namespace NzbDrone.Test.Common
{
    public abstract class LoggingFixtures
    {

        [SetUp]
        public void SetUpBase()
        {
            LogConfiguration.RegisterConsoleLogger(LogLevel.Trace);
            LogConfiguration.RegisterUdpLogger();

            RegisterExceptionVerification();
        }

        private static void RegisterExceptionVerification()
        {
            var exceptionVerification = new ExceptionVerification();
            LogManager.Configuration.AddTarget("ExceptionVerification", exceptionVerification);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, exceptionVerification));
            LogConfiguration.Reload();
        }
    }
}
