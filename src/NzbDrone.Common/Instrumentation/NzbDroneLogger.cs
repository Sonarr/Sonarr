using System;
using System.Diagnostics;
using System.IO;
using LogentriesNLog;
using NLog;
using NLog.Config;
using NLog.Targets;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Instrumentation
{
    public static class NzbDroneLogger
    {
        private static bool _isConfigured;

        static NzbDroneLogger()
        {
            LogManager.Configuration = new LoggingConfiguration();
        }


        public static void Register(IStartupContext startupContext, bool updateApp, bool inConsole)
        {
            if (_isConfigured)
            {
                throw new InvalidOperationException("Loggers have already been registered.");
            }

            _isConfigured = true;

            GlobalExceptionHandlers.Register();

            var appFolderInfo = new AppFolderInfo(startupContext);

            if (Debugger.IsAttached)
            {
                RegisterDebugger();
            }

            //Disabling for now - until its fixed or we yank it out
            //RegisterExceptron();

            if (updateApp)
            {
                RegisterUpdateFile(appFolderInfo);
                RegisterLogEntries();
            }
            else
            {
                if (inConsole && (OsInfo.IsNotWindows || RuntimeInfoBase.IsUserInteractive))
                {
                    RegisterConsole();
                }

                RegisterAppFile(appFolderInfo);
            }

            LogManager.ReconfigExistingLoggers();
        }

        private static void RegisterLogEntries()
        {
            var target = new LogentriesTarget();
            target.Name = "logentriesTarget";
            target.Token = "d3a83ee9-74fb-4045-ad25-a84c1d4d7c81";
            target.LogHostname = true;
            target.Debug = false;

            var loggingRule = new LoggingRule("*", LogLevel.Info, target);
            LogManager.Configuration.AddTarget("logentries", target);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }

        private static void RegisterDebugger()
        {
            DebuggerTarget target = new DebuggerTarget();
            target.Name = "debuggerLogger";
            target.Layout = "[${level}] [${threadid}] ${logger}: ${message} ${onexception:inner=${newline}${newline}${exception:format=ToString}${newline}}";

            var loggingRule = new LoggingRule("*", LogLevel.Trace, target);
            LogManager.Configuration.AddTarget("debugger", target);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }


        private static void RegisterConsole()
        {
            var level = LogLevel.Trace;

            var coloredConsoleTarget = new ColoredConsoleTarget();

            coloredConsoleTarget.Name = "consoleLogger";
            coloredConsoleTarget.Layout = "[${level}] ${logger}: ${message} ${onexception:inner=${newline}${newline}${exception:format=ToString}${newline}}";

            var loggingRule = new LoggingRule("*", level, coloredConsoleTarget);

            LogManager.Configuration.AddTarget("console", coloredConsoleTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }

        const string FILE_LOG_LAYOUT = @"${date:format=yy-M-d HH\:mm\:ss.f}|${level}|${logger}|${message}${onexception:inner=${newline}${newline}${exception:format=ToString}${newline}}";

        private static void RegisterAppFile(IAppFolderInfo appFolderInfo)
        {
            RegisterAppFile(appFolderInfo, "appFileInfo", "sonarr.txt", 5);
            RegisterAppFile(appFolderInfo, "appFileDebug", "sonarr.debug.txt", 50);
            RegisterAppFile(appFolderInfo, "appFileTrace", "sonarr.trace.txt", 50);
        }

        private static LoggingRule RegisterAppFile(IAppFolderInfo appFolderInfo, string name, string fileName, int maxArchiveFiles)
        {
            var fileTarget = new NzbDroneFileTarget();

            fileTarget.Name = name;
            fileTarget.FileName = Path.Combine(appFolderInfo.GetLogFolder(), fileName);
            fileTarget.AutoFlush = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.ConcurrentWrites = false;
            fileTarget.ConcurrentWriteAttemptDelay = 50;
            fileTarget.ConcurrentWriteAttempts = 10;
            fileTarget.ArchiveAboveSize = 1024000;
            fileTarget.MaxArchiveFiles = maxArchiveFiles;
            fileTarget.EnableFileDelete = true;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Rolling;
            fileTarget.Layout = FILE_LOG_LAYOUT;

            var loggingRule = new LoggingRule("*", LogLevel.Trace, fileTarget);

            LogManager.Configuration.AddTarget(name, fileTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);

            return loggingRule;
        }

        private static void RegisterUpdateFile(IAppFolderInfo appFolderInfo)
        {
            var fileTarget = new FileTarget();

            fileTarget.Name = "updateFileLogger";
            fileTarget.FileName = Path.Combine(appFolderInfo.GetUpdateLogFolder(), DateTime.Now.ToString("yyyy.MM.dd-HH.mm") + ".txt");
            fileTarget.AutoFlush = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.ConcurrentWrites = false;
            fileTarget.ConcurrentWriteAttemptDelay = 50;
            fileTarget.ConcurrentWriteAttempts = 100;
            fileTarget.Layout = FILE_LOG_LAYOUT;

            var loggingRule = new LoggingRule("*", LogLevel.Trace, fileTarget);

            LogManager.Configuration.AddTarget("updateFile", fileTarget);
            LogManager.Configuration.LoggingRules.Add(loggingRule);
        }

        private static void RegisterExceptron()
        {
            var exceptronTarget = new ExceptronTarget();
            var rule = new LoggingRule("*", LogLevel.Warn, exceptronTarget);

            LogManager.Configuration.AddTarget("ExceptronTarget", exceptronTarget);
            LogManager.Configuration.LoggingRules.Add(rule);
        }


        public static Logger GetLogger(Type obj)
        {
            return LogManager.GetLogger(obj.Name.Replace("NzbDrone.", ""));
        }

        public static Logger GetLogger(object obj)
        {
            return GetLogger(obj.GetType());
        }

    }
}