using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;

namespace NzbDrone.Test.Common
{
    public abstract class LoggingTest
    {
        protected Logger TestLogger = LogManager.GetLogger("TestLogger");

        protected static void InitLogging()
        {
            if (LogManager.Configuration == null || LogManager.Configuration is XmlLoggingConfiguration)
            {
                LogManager.Configuration = new LoggingConfiguration();
                var consoleTarget = new ConsoleTarget { Layout = "${level}: ${message} ${exception}" };
                LogManager.Configuration.AddTarget(consoleTarget.GetType().Name, consoleTarget);
                LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, consoleTarget));

                RegisterExceptionVerification();
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
            if (TestContext.CurrentContext.Result.State == TestState.Success)
            {
                ExceptionVerification.AssertNoUnexcpectedLogs();
            }
        }
    }
}
