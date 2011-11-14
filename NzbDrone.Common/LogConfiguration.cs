using System;
using System.Diagnostics;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NzbDrone.Common
{
    public static class LogConfiguration
    {
        static LogConfiguration()
        {
            if (EnviromentProvider.IsProduction)
            {
                LogManager.ThrowExceptions = false;
            }
            else
            {
                LogManager.ThrowExceptions = true;
            }

            if (LogManager.Configuration == null)
            {
                LogManager.Configuration = new LoggingConfiguration();
            }
        }

        public static void RegisterConsoleLogger(LogLevel minLevel, string loggerNamePattern = "*")
        {
            try
            {
                var consoleTarget = new ConsoleTarget();
                consoleTarget.Layout = "${message} ${exception}";
                LogManager.Configuration.AddTarget(consoleTarget.GetType().Name, consoleTarget);
                LogManager.Configuration.LoggingRules.Add(new LoggingRule(loggerNamePattern, minLevel, consoleTarget));

                LogManager.ConfigurationReloaded += (sender, args) => RegisterConsoleLogger(minLevel, loggerNamePattern);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                if (LogManager.ThrowExceptions)
                    throw;
            }

        }

        public static void RegisterUdpLogger()
        {
            try
            {
                var udpTarget = new ChainsawTarget();
                udpTarget.Address = "udp://127.0.0.1:20480";
                udpTarget.IncludeCallSite = true;
                udpTarget.IncludeSourceInfo = true;
                udpTarget.IncludeNLogData = true;
                udpTarget.IncludeNdc = true;
                LogManager.Configuration.AddTarget(udpTarget.GetType().Name, udpTarget);
                LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, udpTarget));

                LogManager.ConfigurationReloaded += (sender, args) => RegisterUdpLogger();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                if (LogManager.ThrowExceptions)
                    throw;
            }

        }

        public static void RegisterExceptioneer()
        {
            if (EnviromentProvider.IsProduction)
            {
                try
                {
                    var exTarget = new ExceptioneerTarget();
                    LogManager.Configuration.AddTarget("Exceptioneer", exTarget);
                    LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, exTarget));

                    LogManager.ConfigurationReloaded += (sender, args) => RegisterExceptioneer();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static void Reload()
        {
            var sw = Stopwatch.StartNew();
            LogManager.Configuration.Reload();
            LogManager.ReconfigExistingLoggers();
            sw.Stop();
        }
    }
}
