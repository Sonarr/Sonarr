using System;
using NLog;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Common.NlogTargets;

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
                var udpTarget = new NLogViewerTarget();
                udpTarget.Address = "udp://127.0.0.1:20480";
                udpTarget.IncludeCallSite = true;
                udpTarget.IncludeSourceInfo = true;
                udpTarget.IncludeNLogData = true;
                udpTarget.IncludeNdc = true;
                udpTarget.Parameters.Add(new NLogViewerParameterInfo
                                             {
                                                 Name = "Exception",
                                                 Layout = "${exception:format=ToString}"
                                             });

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


        private static FileTarget GetBaseTarget()
        {
            var fileTarget = new FileTarget();
            fileTarget.AutoFlush = true;
            fileTarget.ConcurrentWrites = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.ConcurrentWriteAttemptDelay = 50;
            fileTarget.ConcurrentWriteAttempts = 200;

            fileTarget.Layout = @"${date:format=yy-M-d HH\:mm\:ss.f}|${replace:searchFor=NzbDrone.:replaceWith=:inner=${logger}}|${level}|${message}|${exception:format=ToString}";

            return fileTarget;
        }

        public static void RegisterFileLogger(string fileName, LogLevel level)
        {
            var fileTarget = GetBaseTarget();
            fileTarget.FileName = fileName;

            LogManager.Configuration.AddTarget(Guid.NewGuid().ToString(), fileTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", level, fileTarget));
        }

        public static void RegisterRollingFileLogger(string fileName, LogLevel level)
        {
            var fileTarget = GetBaseTarget();
            fileTarget.FileName = fileName;
            fileTarget.ArchiveAboveSize = 512000; //500K x 2
            fileTarget.MaxArchiveFiles = 1;
            fileTarget.EnableFileDelete = true;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Rolling;

            LogManager.Configuration.AddTarget(Guid.NewGuid().ToString(), fileTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", level, fileTarget));
        }

        public static void RegisterRemote()
        {
            //if (EnviromentProvider.IsProduction)
            //{
            //    try
            //    {
            //        var exceptioneerTarget = new ExceptioneerTarget();
            //        LogManager.Configuration.AddTarget("Exceptioneer", exceptioneerTarget);
            //        LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, exceptioneerTarget));
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e);
            //    }
            //}

            try
            {
                var remoteTarget = new RemoteTarget();
                LogManager.Configuration.AddTarget("RemoteTarget", remoteTarget);
                LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, remoteTarget));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            LogManager.ConfigurationReloaded += (sender, args) => RegisterRemote();
        }

        public static void Reload()
        {
            LogManager.Configuration.Reload();
            LogManager.ReconfigExistingLoggers();
        }
    }
}
