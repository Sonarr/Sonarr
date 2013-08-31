using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Instrumentation
{
    public static class LogTargets
    {
        public static void Register(IStartupArguments startupArguments, bool updateApp, bool inConsole)
        {
            var appFolderInfo = new AppFolderInfo(new DiskProvider(), startupArguments);

            LogManager.Configuration = new LoggingConfiguration();

            RegisterExceptron();

            if (updateApp)
            {
                RegisterLoggly();
                RegisterUpdateFile(appFolderInfo);
            }
            else
            {
                if (inConsole && (OsInfo.IsLinux || new RuntimeInfo(null).IsUserInteractive))
                {
                    RegisterConsole();
                }

                RegisterAppFile(appFolderInfo);
            }
        }

        private static void RegisterConsole()
        {
            var level = LogLevel.Trace;

            if (RuntimeInfo.IsProduction)
            {
                level = LogLevel.Info;
            }

            var coloredConsoleTarget = new ColoredConsoleTarget();

            coloredConsoleTarget.Name = "consoleLogger";
            coloredConsoleTarget.Layout = "[${level}] ${logger}: ${message} ${onexception:inner=${newline}${newline}${exception:format=ToString}${newline}}";

            var loggingRule = new LoggingRule("*", level, coloredConsoleTarget);

            LogManager.Configuration.AddTarget("console", coloredConsoleTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);

            LogManager.ConfigurationReloaded += (sender, args) => RegisterConsole();
            LogManager.ReconfigExistingLoggers();
        }


        const string FileLogLayout = @"${date:format=yy-M-d HH\:mm\:ss.f}|${level}|${logger}|${message}${onexception:inner=${newline}${newline}${exception:format=ToString}${newline}}";

        private static void RegisterAppFile(IAppFolderInfo appFolderInfo)
        {
            var fileTarget = new FileTarget();

            fileTarget.Name = "rollingFileLogger";
            fileTarget.FileName = Path.Combine(appFolderInfo.GetLogFolder(), "nzbdrone.txt");
            fileTarget.AutoFlush = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.ConcurrentWrites = false;
            fileTarget.ConcurrentWriteAttemptDelay = 50;
            fileTarget.ConcurrentWriteAttempts = 10;
            fileTarget.ArchiveAboveSize = 1024000;
            fileTarget.MaxArchiveFiles = 5;
            fileTarget.EnableFileDelete = true;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Rolling;
            fileTarget.Layout = FileLogLayout;

            var loggingRule = new LoggingRule("*", LogLevel.Info, fileTarget);

            LogManager.Configuration.AddTarget("appfile", fileTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);

            LogManager.ConfigurationReloaded += (sender, args) => RegisterAppFile(appFolderInfo);
            LogManager.ReconfigExistingLoggers();
        }



        private static void RegisterUpdateFile(IAppFolderInfo appFolderInfo)
        {
            var fileTarget = new FileTarget();

            fileTarget.Name = "updateFileLogger";
            fileTarget.FileName = Path.Combine(appFolderInfo.GetUpdateLogFolder(), DateTime.Now.ToString("yy.MM.d-HH.mm") + ".txt");
            fileTarget.AutoFlush = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.ConcurrentWrites = false;
            fileTarget.ConcurrentWriteAttemptDelay = 50;
            fileTarget.ConcurrentWriteAttempts = 100;
            fileTarget.Layout = FileLogLayout;

            var loggingRule = new LoggingRule("*", LogLevel.Trace, fileTarget);

            LogManager.Configuration.AddTarget("updateFile", fileTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);

            LogManager.ConfigurationReloaded += (sender, args) => RegisterUpdateFile(appFolderInfo);
            LogManager.ReconfigExistingLoggers();
        }

        private static void RegisterExceptron()
        {

            var exceptronTarget = new ExceptronTarget();
            var rule = new LoggingRule("*", LogLevel.Warn, exceptronTarget);

            LogManager.Configuration.AddTarget("ExceptronTarget", exceptronTarget);
            LogManager.Configuration.LoggingRules.Add(rule);
            LogManager.ConfigurationReloaded += (sender, args) => RegisterExceptron();
            LogManager.ReconfigExistingLoggers();
        }


        public static void RegisterLoggly()
        {
            var logglyTarger = new LogglyTarget();

            var rule = new LoggingRule("*", LogLevel.Trace, logglyTarger);

            LogManager.Configuration.AddTarget("LogglyLogger", logglyTarger);
            LogManager.Configuration.LoggingRules.Add(rule);
            LogManager.ConfigurationReloaded += (sender, args) => RegisterLoggly();
            LogManager.ReconfigExistingLoggers();
        }

    }
}