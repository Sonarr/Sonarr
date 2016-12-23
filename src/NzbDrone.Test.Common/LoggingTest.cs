using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Test.Common
{
    public abstract class LoggingTest
    {
        protected static readonly Logger TestLogger = NzbDroneLogger.GetLogger("TestLogger");

        protected static void InitLogging()
        {
            new StartupContext();

            if (LogManager.Configuration == null || LogManager.Configuration.AllTargets.None(c => c is ExceptionVerification))
            {
                LogManager.Configuration = new LoggingConfiguration();
                var consoleTarget = new ConsoleTarget { Layout = "${level}: ${message} ${exception}" };
                LogManager.Configuration.AddTarget(consoleTarget.GetType().Name, consoleTarget);
                LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget));

                RegisterExceptionVerification();

                LogManager.ReconfigExistingLoggers();
            }
        }

        private static void RegisterExceptionVerification()
        {
            var exceptionVerification = new ExceptionVerification();
            LogManager.Configuration.AddTarget("ExceptionVerification", exceptionVerification);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Warn, exceptionVerification));
        }

        [SetUp]
        public void LoggingTestSetup()
        {
            InitLogging();
            ExceptionVerification.Reset();
        }

        [TearDown]
        public void LoggingDownBase()
        {
            //can't use because of a bug in mono with 2.6.2,
            //https://bugs.launchpad.net/nunitv2/+bug/1076932
            if (BuildInfo.IsDebug && TestContext.CurrentContext.Result.Outcome == ResultState.Success)
            {
                ExceptionVerification.AssertNoUnexpectedLogs();
            }
        }
    }
}
